using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Messages;
using Messages.Arc;
using UnityEngine;

namespace SUS
{
    internal sealed class ClientConnection : IDisposable
    {
        private readonly int _id;
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly Action<int, MessageWrapper> _onMessage;
        private readonly Action<int> _onDisconnect;
        private readonly MessageAccumulator _accumulator;

        // Send queue holds (rentedBuffer, validLength) pairs
        private readonly BlockingCollection<(byte[] buffer, int length)> _sendQueue = new(256);
        private readonly CancellationTokenSource _cts = new();

        private Thread _readThread;
        private Thread _writeThread;

        public ClientConnection(
            int id,
            TcpClient client,
            Action<int, MessageWrapper> onMessage,
            Action<int> onDisconnect)
        {
            _id          = id;
            _client      = client;
            _stream      = client.GetStream();
            _onMessage   = onMessage;
            _onDisconnect = onDisconnect;
            _accumulator = new MessageAccumulator(
                Endianness.Little,
                OnWrapperReceived,
                initialCapacity: 64 * 1024  // 64 KB - grows as needed
            );
        }

        // Dispatch received wrapper to the main-thread queue, then dispose it
        // after all subscribers have seen it. SusConnection copies Data before
        // dispatching so pooled memory is safe to return here.
        private void OnWrapperReceived(MessageWrapper wrapper)
        {
            _onMessage(_id, wrapper);
            // Wrapper is pooled — return backing array now that onMessage has taken a copy
            wrapper.Dispose();
        }

        public void Start()
        {
            _readThread  = new Thread(ReadLoop)  { IsBackground = true, Name = $"SUS-Read-{_id}"  };
            _writeThread = new Thread(WriteLoop) { IsBackground = true, Name = $"SUS-Write-{_id}" };
            _readThread.Start();
            _writeThread.Start();
        }

        private void ReadLoop()
        {
            // Rent a reusable read buffer — no per-read allocation
            var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
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
                Debug.LogWarning($"[SUS] Client {_id} read error: {ex.Message}");
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
                Dispose();
            }
        }

        private void WriteLoop()
        {
            try
            {
                foreach (var (buffer, length) in _sendQueue.GetConsumingEnumerable(_cts.Token))
                {
                    try
                    {
                        _stream.Write(buffer, 0, length);
                        // No Flush() — TCP batches large frames (JPEGs) fine without it
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SUS] Client {_id} write error: {ex.Message}");
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
                Debug.LogWarning($"[SUS] Send dropped — client {_id} disconnected");
                return;
            }

            // ImageFrame skips CRC — bulk data, TCP guarantees delivery
            var includeCrc = wrapper.Type != MessageType.ImageFrame;
            var (buffer, length) = MessageWriter.WritePooled(
                wrapper.Type,
                wrapper.Data.AsSpan(0, wrapper.Length),
                Endianness.Little,
                includeCrc
            );

            if (!_sendQueue.TryAdd((buffer, length)))
            {
                // Queue full — drop and return buffer immediately
                ArrayPool<byte>.Shared.Return(buffer);
                Debug.LogWarning($"[SUS] Send queue full — frame dropped for client {_id}");
            }
        }

        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

            _cts.Cancel();
            _sendQueue.CompleteAdding();

            // Drain and return any queued buffers
            while (_sendQueue.TryTake(out var entry))
                ArrayPool<byte>.Shared.Return(entry.buffer);

            try { _stream.Close();  } catch { /* ignored */ }
            try { _client.Close();  } catch { /* ignored */ }

            _accumulator.Dispose();
            _onDisconnect(_id);
        }
    }
}