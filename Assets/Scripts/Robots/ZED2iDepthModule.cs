using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Collections.Concurrent;
using Google.FlatBuffers;
using Messages;
using SUS;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Robots
{
    public class ZED2iDepthTCPSender : MonoBehaviour
    {
        [Header("References")]
        public UniversalRendererData rendererData;

        [Header("Stream Settings")]
        public int width  = 1280;
        public int height = 720;

        private ZED2iDepthFeature       depthFeature;
        private Thread                  sendThread;
        private bool                    running;

        private readonly ConcurrentQueue<float[]> frameQueue = new();

        void Start()
        {
            foreach (var feature in rendererData.rendererFeatures)
                if (feature is ZED2iDepthFeature f) { depthFeature = f; break; }

            if (depthFeature == null)
            {
                return;
            }

            running    = true;
            sendThread = new Thread(SendLoop) { IsBackground = true };
            sendThread.Start();
        }

        void LateUpdate()
        {
            if (depthFeature?.DepthRTHandle?.rt == null) return;

            AsyncGPUReadback.Request(
                depthFeature.DepthRTHandle.rt,
                0,
                TextureFormat.RFloat,
                OnDepthReadback
            );
        }

        void OnDepthReadback(AsyncGPUReadbackRequest req)
        {
            if (req.hasError || !running) return;

            float[] raw = req.GetData<float>().ToArray();
            while (frameQueue.Count > 0) frameQueue.TryDequeue(out _);
            frameQueue.Enqueue(raw);
        }

        void SendLoop()
        {
            while (running)
            {
                if (frameQueue.TryDequeue(out float[] raw))
                {
                    try
                    {
                        var flipped = new float[raw.Length];
                        for (int y = 0; y < height; y++)
                        {
                            int srcRow = y * width;
                            int dstRow = (height - 1 - y) * width;
                            Array.Copy(raw, srcRow, flipped, dstRow, width);
                        }

                        // Convert to bytes for compression
                        byte[] floatBytes = new byte[flipped.Length * sizeof(float)];
                        Buffer.BlockCopy(flipped, 0, floatBytes, 0, floatBytes.Length);

                        // Compress
                        byte[] compressed;
                        using (var ms = new MemoryStream())
                        {
                            using (var gz = new DeflateStream(ms, System.IO.Compression.CompressionLevel.Fastest))
                                gz.Write(floatBytes, 0, floatBytes.Length);
                            compressed = ms.ToArray();
                        }

                        var builder    = new FlatBufferBuilder(compressed.Length + 64);
                        var dataOffset = ZedFrame.CreateDataVector(builder, compressed);
                        var frameOffset = ZedFrame.CreateZedFrame(
                            builder,
                            (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            width,
                            height,
                            dataOffset
                        );
                        builder.Finish(frameOffset.Value);

                        var wrapper = MessageWrapper.From(
                            MessageType.ZedDepth,
                            builder.SizedByteArray()
                        );
                        SusConnection.Instance.Broadcast(wrapper);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"ZED2iDepthTCPSender: Send failed — {e.Message}");
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        void OnDestroy()
        {
            running = false;
            sendThread?.Join(500);
        }
    }
}