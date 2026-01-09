using System.IO;
using Messages;
using Messages.Arc;
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
                }
            }
            catch
            {
                // ignored
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
                    _stream.Write(frame, 0, frame.Length);
                    _stream.Flush();
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                Dispose();
            }
        }

        public void Send(MessageWrapper wrapper)
        {
            if (_sendQueue.IsAddingCompleted)
            {
                Debug.LogWarning($"[SEND DROPPED] client {_id}");
                return;
            }

            // Convert to bytes
            _sendQueue.Add(MessageWriter.Write(wrapper.Type, wrapper.Data, Endianness.Little));
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
                // ignored
            }

            try
            {
                _client.Close();
            }
            catch
            {
                // ignored
            }

            _onDisconnect(_id);
        }
    }
}