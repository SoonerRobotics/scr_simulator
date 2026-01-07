using SUS.Utils;
using UnityEngine;

namespace SUS
{
    using System;
    using System.Collections.Concurrent;
    using System.Net.Sockets;
    using System.Threading;

    internal sealed class ClientConnection : IDisposable
    {
        private readonly int _id;
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly NetworkBinaryReader _reader;
        private readonly NetworkBinaryWriter _writer;

        private readonly Action<int, byte[]> _onFrame;
        private readonly Action<int> _onDisconnect;

        private readonly BlockingCollection<byte[]> _sendQueue = new(256);
        private readonly CancellationTokenSource _cts = new();

        private Thread _readThread;
        private Thread _writeThread;

        public ClientConnection(
            int id,
            TcpClient client,
            Action<int, byte[]> onFrame,
            Action<int> onDisconnect)
        {
            _id = id;
            _client = client;
            _stream = client.GetStream();
            _reader = new NetworkBinaryReader(_stream);
            _writer = new NetworkBinaryWriter(_stream);
            _onFrame = onFrame;
            _onDisconnect = onDisconnect;
        }

        public void Start()
        {
            _readThread = new Thread(ReadLoop) { IsBackground = true };
            _writeThread = new Thread(WriteLoop) { IsBackground = true };

            _readThread.Start();
            _writeThread.Start();
        }

        private void ReadLoop()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    // 🔥 FRAME FORMAT ASSUMPTION:
                    // int length (big-endian) + payload
                    int length = _reader.ReadInt();
                    if (length <= 0 || length > 10_000_000)
                        throw new Exception("Invalid frame length");

                    byte[] frame = _reader.ReadBytes(length);
                    _onFrame(_id, frame);
                }
            }
            catch
            {
            }
            finally
            {
                Dispose();
            }
        }

        private void WriteLoop()
        {
            try
            {
                foreach (var frame in _sendQueue.GetConsumingEnumerable(_cts.Token))
                {
                    _writer.WriteInt32BE(frame.Length);
                    _writer.Write(frame);
                    _writer.Flush();
                }
            }
            catch
            {
            }
            finally
            {
                Dispose();
            }
        }

        public void Send(byte[] frame)
        {
            if (_sendQueue.IsAddingCompleted)
            {
                Debug.LogWarning($"[SEND DROPPED] client {_id}");
                return;
            }

            _sendQueue.Add(frame);
        }

        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

            _cts.Cancel();
            _sendQueue.CompleteAdding();

            try
            {
                _stream.Close();
            }
            catch
            {
            }

            try
            {
                _client.Close();
            }
            catch
            {
            }

            _onDisconnect(_id);
        }
    }
}