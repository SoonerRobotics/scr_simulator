using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SUS;
using SUS.Packets;
using SUS.Packets.IGVC._2026;
using SUS.Utils;
using UnityEngine;

public class SUSConnection : MonoBehaviour
{
    private static SUSConnection _mInstance;

    public static SUSConnection GetOrCreateInstance()
    {
        if (_mInstance != null)
        {
            return _mInstance;
        }
        
        _mInstance = new GameObject("@SUSConnection").AddComponent<SUSConnection>();
        return _mInstance;
    }

    private TcpListener _mListener;
    private TcpClient _mClient;
    private NetworkStream _mStream;
    private BinaryWriter _mWriter;
    private BinaryReader _mReader;

    private Thread _mReadThread;
    private Thread _mWriteThread;
    private bool _mConnected;
    
    private readonly BlockingCollection<IOutgoingPacket> _mSendQueue = new(boundedCapacity: 256);
    private readonly ConcurrentQueue<IncomingPacket> _mIncomingQueue = new();
    private readonly MessageDispatcher _mDispatcher = new();
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        _mListener = new TcpListener(IPAddress.Any, 4001); // TODO: Move port to variable
        _mListener.Start();

        _mListener.BeginAcceptTcpClient(OnClientAccepted, null);
    }

    private void OnClientAccepted(IAsyncResult ar)
    {
        _mClient = _mListener.EndAcceptTcpClient(ar);
        _mStream = _mClient.GetStream();
        
        _mWriter = new BinaryWriter(_mStream);
        _mReader = new BinaryReader(_mStream);
        _mConnected = true;

        _mReadThread = new Thread(ReadLoop) { IsBackground = true };
        _mWriteThread = new Thread(WriteLoop) { IsBackground = true };

        Debug.Log("Client Connected");
        
        _mReadThread.Start();
        _mWriteThread.Start();
    }

    private void ReadLoop()
    {
        try
        {
            while (_mConnected)
            {
                var msgType = _mReader.ReadInt32BE(); // TODO: Convert to enum

                switch ((PacketType)msgType)
                {
                    case PacketType.MotorInput_2026:
                        _mIncomingQueue.Enqueue(IncomingMotorInput.Read(_mReader));
                        break;
                    default:
                        // throw new ArgumentOutOfRangeException();
                        Debug.LogError("Unknown Packet Type: " + msgType);
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Read Thread Stopped: {e.Message}");
            _mConnected = false;
        }
    }

    private void WriteLoop()
    {
        try
        {
            while (_mConnected)
            {
                if (_mSendQueue.Count == 0) continue;
                
                var packet = _mSendQueue.Take();
                packet.Write(_mWriter);
                _mWriter.Flush();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Write Thread Stopped: {e.Message}");
            _mConnected = false;
        }
    }
    
    private void Update()
    {
        for (int i = 0; i < 5; i++)
        {
            if (!_mIncomingQueue.TryDequeue(out var packet))
            {
                break;
            }

            HandlePacket(packet);
        }
    }

    private void HandlePacket(IncomingPacket packet)
    {
        switch (packet)
        {
            case IncomingMotorInput motorInputPacket:
                _mDispatcher.Dispatch(motorInputPacket);
                break;
        }
    }

    private void OnDestroy()
    {
        _mConnected = false;

        _mSendQueue.CompleteAdding();
        
        _mStream?.Close();
        _mClient?.Close();
        _mListener?.Stop();

        _mReadThread?.Join(500);
        _mWriteThread?.Join(500);
    }

    // publics

    public void Write(IOutgoingPacket packet)
    {
        _mSendQueue.Add(packet);
    }
    
    public void Subscribe<T>(Action<T> callback) where T : IncomingPacket
    {
        _mDispatcher.Subscribe(callback);
    }

    public void Unsubscribe<T>(Action<T> callback) where T : IncomingPacket
    {
        _mDispatcher.Unsubscribe(callback);
    }
}
