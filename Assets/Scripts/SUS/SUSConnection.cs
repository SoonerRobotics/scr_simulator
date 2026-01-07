using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
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
        private readonly ConcurrentQueue<(int clientId, byte[] frame)> _incomingFrames = new();

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
                    int id = _nextClientId++;

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
                catch (ObjectDisposedException) { }
                finally
                {
                    BeginAccept(); // continue accepting
                }
            }, null);
        }

        private void OnFrameReceived(int clientId, byte[] frame)
        {
            _incomingFrames.Enqueue((clientId, frame));
        }

        private void OnClientDisconnected(int clientId)
        {
            _clients.TryRemove(clientId, out _);
            Debug.Log($"Client {clientId} disconnected");
        }

        private void Update()
        {
            for (int i = 0; i < 10; i++)
            {
                if (!_incomingFrames.TryDequeue(out var entry))
                    break;

                HandleFrame(entry.clientId, entry.frame);
            }
        }

        private void HandleFrame(int clientId, byte[] frame)
        {
            // 🔥 THIS IS WHERE YOUR FLATBUFFER / FRAME STUFF GOES
            // Example:
            // FrameRouter.Route(clientId, frame);

            Debug.Log($"Received frame from client {clientId}, {frame.Length} bytes");
        }

        private void OnDestroy()
        {
            foreach (var client in _clients.Values)
                client.Dispose();

            _listener?.Stop();
        }

        // ========= Public API =========

        public void SendToClient(int clientId, byte[] frame)
        {
            if (_clients.TryGetValue(clientId, out var client))
                client.Send(frame);
        }
        
        public void Broadcast(byte[] frame)
        {
            Debug.Log($"Broadcast {frame.Length} bytes to {_clients.Count} clients");
            foreach (var client in _clients.Values)
                client.Send(frame);
        }
    }
}
