using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Google.FlatBuffers;
using Messages;
using SUS;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ZstdSharp;

namespace Robots
{
    public class ZED2iDepthTCPSender : MonoBehaviour
    {
        [Header("References")]
        public UniversalRendererData rendererData;

        [Header("Stream Settings")]
        public int width = 1280;
        public int height = 720;

        [Header("Compression")]
        [Range(1, 22)]
        public int zstdLevel = 1;
        private ZED2iDepthFeature _depthFeature;
        private CancellationTokenSource _cts;
        private BlockingCollection<(byte[] buffer, int floatCount)> _frameQueue;

        private void Start()
        {
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature is ZED2iDepthFeature f)
                {
                    _depthFeature = f;
                    break;
                }
            }

            if (_depthFeature == null)
            {
                Debug.LogError("[ZED2i] ZED2iDepthFeature not found on rendererData");
                return;
            }

            _frameQueue = new BlockingCollection<(byte[], int)>(boundedCapacity: 1);
            _cts = new CancellationTokenSource();

            Task.Run(() => SendLoop(_cts.Token));
        }

        private void LateUpdate()
        {
            if (_depthFeature?.DepthRTHandle?.rt == null)
            {
                return;
            }

            AsyncGPUReadback.Request(
                _depthFeature.DepthRTHandle.rt,
                0,
                TextureFormat.RFloat,
                OnDepthReadback
            );
        }

        private void OnDepthReadback(AsyncGPUReadbackRequest req)
        {
            if (req.hasError || _cts.IsCancellationRequested)
            {
                return;
            }

            var floatCount = width * height;
            if (req.GetData<float>().Length != floatCount)
            {
                Debug.LogWarning($"[ZED2i] Unexpected readback size {req.GetData<float>().Length}, expected {floatCount}. Skipping.");
                return;
            }

            var byteCount = floatCount * sizeof(float);
            var buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            req.GetData<byte>().CopyTo(buffer);

            while (!_frameQueue.TryAdd((buffer, floatCount)))
            {
                if (_frameQueue.TryTake(out var old))
                {
                    ArrayPool<byte>.Shared.Return(old.buffer);
                }
            }
        }

        private void SendLoop(CancellationToken token)
        {
            using var compressor = new Compressor(zstdLevel);

            while (!token.IsCancellationRequested)
            {
                (byte[] buffer, int floatCount) frame;
                try
                {
                    frame = _frameQueue.Take(token);
                }

                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    var byteCount = frame.floatCount * sizeof(float);
                    var rawSpan = frame.buffer.AsSpan(0, byteCount);

                    FlipVerticalInPlace(rawSpan, width, height);

                    var maxCompressed = (int)Compressor.GetCompressBound(byteCount);
                    var compBuf = ArrayPool<byte>.Shared.Rent(maxCompressed);
                    try
                    {
                        var compressedLen = compressor.Wrap(rawSpan, compBuf.AsSpan(0, maxCompressed));

                        var builder = new FlatBufferBuilder(compressedLen + 64);
                        var dataOffset = ZedFrame.CreateDataVectorBlock(
                            builder,
                            new ArraySegment<byte>(compBuf, 0, compressedLen)
                        );
                        var frameOffset = ZedFrame.CreateZedFrame(
                            builder,
                            (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            width,
                            height,
                            dataOffset
                        );
                        builder.Finish(frameOffset.Value);

                        var wrapper = MessageWrapper.From(MessageType.ZedDepth, builder.SizedByteArray());
                        SusConnection.Instance.Broadcast(wrapper);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(compBuf);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ZED2i] Send failed — {e.Message}");
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(frame.buffer);
                }
            }
        }

        private static void FlipVerticalInPlace(Span<byte> bytes, int w, int h)
        {
            var floats = MemoryMarshal.Cast<byte, float>(bytes);
            var mid = h / 2;

            for (var y = 0; y < mid; y++)
            {
                var top = floats.Slice(y * w, w);
                var bot = floats.Slice((h - 1 - y) * w, w);

                var tmp = ArrayPool<float>.Shared.Rent(w);
                try
                {
                    top.CopyTo(tmp);
                    bot.CopyTo(top);
                    tmp.AsSpan(0, w).CopyTo(bot);
                }
                finally
                {
                    ArrayPool<float>.Shared.Return(tmp);
                }
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _frameQueue?.CompleteAdding();

            // Return any frame that never got processed
            if (_frameQueue != null)
            {
                while (_frameQueue.TryTake(out var leftover))
                    ArrayPool<byte>.Shared.Return(leftover.buffer);
            }

            _cts?.Dispose();
        }
    }
}