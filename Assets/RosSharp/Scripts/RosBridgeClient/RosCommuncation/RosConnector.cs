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
using RosSharp.RosBridgeClient.Protocols;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class RosConnector : MonoBehaviour
    {
        public static RosConnector instance;

        public int Timeout = 3;

        public RosSocket RosSocket { get; private set; }
        public RosSocket.SerializerEnum Serializer;
        public Protocol Protocol;
        public string RosBridgeServerUrl = "ws://192.168.0.1:9090";

        public bool Connected { get; private set; } = false;

        public void Awake()
        {
            if (instance)
                return;

            instance = this;
            RosSocket = ConnectToRos(Protocol, RosBridgeServerUrl, OnConnected, OnClosed, Serializer);
            DontDestroyOnLoad(this);
            Connect();
        }

        private void Connect()
        {
            RosSocket.protocol.Connect();
        }

        public static RosSocket ConnectToRos(Protocol protocolType, string serverUrl, EventHandler onConnected = null, EventHandler onClosed = null, RosSocket.SerializerEnum serializer = RosSocket.SerializerEnum.JSON)
        {
            IProtocol protocol = ProtocolInitializer.GetProtocol(protocolType, serverUrl);
            protocol.OnConnected += onConnected;
            protocol.OnClosed += onClosed;

            return new RosSocket(protocol, serializer);
        }

        private void OnApplicationQuit()
        {
            if (RosSocket != null)
                RosSocket.Close();
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Connected = true;
            Debug.Log("Connected to RosBridge: " + RosBridgeServerUrl);
        }

        private async void OnClosed(object sender, EventArgs e)
        {
            Connected = false;
            Debug.Log("Disconnected from RosBridge: " + RosBridgeServerUrl + ". Retrying in " + Timeout + " seconds.");
            await System.Threading.Tasks.Task.Delay(Timeout * 1000);
            Connect();
        }
    }
}