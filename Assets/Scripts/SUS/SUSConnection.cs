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

        // Queue holds owned (non-pooled) wrappers safe to read on the main thread
        private readonly ConcurrentQueue<(int clientId, MessageWrapper wrapper)> _incomingFrames = new();
        private readonly Dictionary<MessageType, List<Action<MessageWrapper>>> _subscriptions = new();

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            _listener = new TcpListener(IPAddress.Any, 4001);
            _listener.Start();

            BeginAccept();
            Debug.Log("[SUS] Listening on port 4001");
        }

        private void BeginAccept()
        {
            _listener.BeginAcceptTcpClient(ar =>
            {
                try
                {
                    var client = _listener.EndAcceptTcpClient(ar);
                    var id     = _nextClientId++;

                    var connection = new ClientConnection(id, client, OnMessageReceived, OnClientDisconnected);
                    _clients[id]  = connection;

                    Debug.Log($"[SUS] Client {id} connected from {((System.Net.Sockets.TcpClient)client).Client.RemoteEndPoint}");
                    connection.Start();
                }
                catch (ObjectDisposedException) { }
                finally
                {
                    BeginAccept();
                }
            }, null);
        }

        /// <summary>
        /// Called from a background read thread.
        /// The wrapper is pooled — copy its payload to an owned wrapper for the main thread queue.
        /// </summary>
        private void OnMessageReceived(int clientId, MessageWrapper wrapper)
        {
            // Copy payload out of the pooled buffer into an owned array
            var ownedData = new byte[wrapper.Length];
            wrapper.Data.AsSpan(0, wrapper.Length).CopyTo(ownedData);
            var owned = MessageWrapper.From(wrapper.Type, ownedData);

            _incomingFrames.Enqueue((clientId, owned));
            // wrapper.Dispose() is called by ClientConnection.OnWrapperReceived after this returns
        }

        private void OnClientDisconnected(int clientId)
        {
            _clients.TryRemove(clientId, out _);
            Debug.Log($"[SUS] Client {clientId} disconnected");
        }

        private void Update()
        {
            // Process up to 20 messages per frame — raise if needed for high FPS streams
            for (var i = 0; i < 20; i++)
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
                try   { handler(wrapper); }
                catch (Exception ex) { Debug.LogError($"[SUS] Subscriber threw on {wrapper.Type}: {ex}"); }
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