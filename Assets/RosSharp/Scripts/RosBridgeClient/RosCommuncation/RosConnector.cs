/*
© Siemens AG, 2017-2019
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

<http://www.apache.org/licenses/LICENSE-2.0>.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Threading;
using RosSharp.RosBridgeClient.Protocols;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class RosConnector : MonoBehaviour
    {
        public static RosConnector instance;
        public float SecondsTimeout = 1.0f;

        public RosSocket RosSocket { get; private set; }
        private Thread SocketThread;
        public RosSocket.SerializerEnum Serializer;
        public Protocol protocol;
        public string RosBridgeServerUrl = "ws://192.168.0.1:9090";

        public ManualResetEvent IsConnected { get; private set; }

        public virtual void Awake()
        {
            if (instance)
                return;

            instance = this;
            DontDestroyOnLoad(this);
            
            IsConnected = new ManualResetEvent(false);
            SocketThread = new Thread(ConnectAndWait);
            SocketThread.Start();
        }

        protected void ConnectAndWait()
        {
            RosSocket = ConnectToRos(protocol, RosBridgeServerUrl, OnConnected, OnClosed, Serializer);

            while (!IsConnected.WaitOne((int)(SecondsTimeout * 1000)))
                if (RosSocket == null) {
                    break;
                }
                Debug.LogWarning("Failed to connect to RosBridge at: " + RosBridgeServerUrl);
        }

        public static RosSocket ConnectToRos(Protocol protocolType, string serverUrl, EventHandler onConnected = null, EventHandler onClosed = null, RosSocket.SerializerEnum serializer = RosSocket.SerializerEnum.Microsoft)
        {
            IProtocol protocol = ProtocolInitializer.GetProtocol(protocolType, serverUrl);
            protocol.OnConnected += onConnected;
            protocol.OnClosed += onClosed;

            return new RosSocket(protocol, serializer);
        }

        private void OnApplicationQuit()
        {
            if (RosSocket != null) {
                RosSocket.Close();
            }

            // Necessary as this is also our flag to stop running threads
            RosSocket = null;

            if (SocketThread != null) {
                SocketThread.Join((int)(SecondsTimeout * 1000));
            }
        }

        private void OnConnected(object sender, EventArgs e)
        {
            IsConnected.Set();
            Debug.Log("Connected to RosBridge: " + RosBridgeServerUrl);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            IsConnected.Reset();
            Debug.Log("Disconnected from RosBridge: " + RosBridgeServerUrl);

            RosSocket.Close();
            RosSocket = null;
            SocketThread.Join((int)(SecondsTimeout * 1000));

            SocketThread = new Thread(ConnectAndWait);
            SocketThread.Start();
        }
    }
}