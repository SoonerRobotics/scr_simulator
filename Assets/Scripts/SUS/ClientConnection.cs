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
        private readonly Action<int, MessageWrapper> _onMessage;
        private readonly Action<int> _onDisconnect;
        private readonly MessageAccumulator _accumulator;
        private readonly BlockingCollection<byte[]> _sendQueue = new(256);
        private readonly CancellationTokenSource _cts = new();
        private Thread _readThread;
        private Thread _writeThread;

        public ClientConnection(
            int id,
            TcpClient client,
            Action<int, MessageWrapper> onMessage,
            Action<int> onDisconnect)
        {
            _id = id;
            _client = client;
            _stream = client.GetStream();
            _onMessage = onMessage;
            _onDisconnect = onDisconnect;
            _accumulator = new MessageAccumulator(
                Endianness.Little,
                wrapper => _onMessage(_id, wrapper)
            );
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
            var buffer = new byte[4096];
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var bytesRead = _stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    _accumulator.Append(buffer.AsSpan(0, bytesRead));
                }
            }
            catch (Exception) when (_cts.IsCancellationRequested) { }
            catch (Exception ex)
            {
                Debug.LogWarning($"Client {_id} read error: {ex.Message}");
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