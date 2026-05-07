using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Messages;
using UnityEngine;

namespace SUS
{
    public class SusConnection : MonoBehaviour
    {
        private static SusConnection _instance;

        public static SusConnection Instance =>
            _instance ??= new GameObject("@SUSConnection").AddComponent<SusConnection>();

        private TcpListener _listener;
        private int _nextClientId = 1;

        private readonly ConcurrentDictionary<int, ClientConnection> _clients = new();
        private readonly ConcurrentDictionary<int, MessageAccumulator> _accumulators = new();
        private readonly ConcurrentQueue<(int clientId, MessageWrapper wrapper)> _incomingFrames = new();
        private readonly Dictionary<MessageType, List<Action<MessageWrapper>>> _subscriptions = new();

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            _listener = new TcpListener(IPAddress.Any, 4001);
            _listener.Start();

            BeginAccept();
            Debug.Log("SUSConnection listening on port 4001");
        }

        private void BeginAccept()
        {
            _listener.BeginAcceptTcpClient(ar =>
            {
                try
                {
                    var client = _listener.EndAcceptTcpClient(ar);
                    var id = _nextClientId++;

                    _accumulators[id] = new MessageAccumulator(
                        Endianness.Little,
                        wrapper => _incomingFrames.Enqueue((id, wrapper))
                    );

                    var connection = new ClientConnection(
                        id,
                        client,
                        OnFrameReceived,
                        OnClientDisconnected
                    );

                    _clients[id] = connection;

                    Debug.Log($"Client {id} connected");
                    connection.Start();
                }
                catch (ObjectDisposedException)
                {
                }
                finally
                {
                    BeginAccept();
                }
            }, null);
        }

        private void OnFrameReceived(int clientId, byte[] frame)
        {
            if (_accumulators.TryGetValue(clientId, out var accumulator))
                accumulator.Append(frame);
        }

        private void OnClientDisconnected(int clientId)
        {
            _clients.TryRemove(clientId, out _);
            _accumulators.TryRemove(clientId, out _);
            Debug.Log($"Client {clientId} disconnected");
        }

        private void Update()
        {
            for (var i = 0; i < 10; i++)
            {
                if (!_incomingFrames.TryDequeue(out var entry))
                    break;

                DispatchToSubscribers(entry.wrapper);
            }
        }

        private void DispatchToSubscribers(MessageWrapper wrapper)
        {
            if (!_subscriptions.TryGetValue(wrapper.Type, out var handlers)) return;

            foreach (var handler in handlers)
            {
                try
                {
                    handler(wrapper);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Subscriber threw on {wrapper.Type}: {ex}");
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var client in _clients.Values)
                client.Dispose();

            _listener?.Stop();
        }

        public void Broadcast(MessageWrapper wrapper)
        {
            foreach (var client in _clients.Values)
                client.Send(wrapper);
        }

        public IDisposable Subscribe(MessageType type, Action<MessageWrapper> handler)
        {
            if (!_subscriptions.TryGetValue(type, out var list))
            {
                list = new List<Action<MessageWrapper>>();
                _subscriptions[type] = list;
            }

            list.Add(handler);

            return new Subscription(() => Unsubscribe(type, handler));
        }

        private void Unsubscribe(MessageType type, Action<MessageWrapper> handler)
        {
            if (_subscriptions.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        private sealed class Subscription : IDisposable
        {
            private Action _onDispose;

            public Subscription(Action onDispose) => _onDispose = onDispose;

            public void Dispose()
            {
                _onDispose?.Invoke();
                _onDispose = null;
            }
        }
    }
}