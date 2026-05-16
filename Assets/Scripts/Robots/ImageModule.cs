using System;
using System.Buffers;
using System.Threading.Tasks;
using Google.FlatBuffers;
using Messages;
using SUS;
using UnityEngine;
using UnityEngine.Rendering;

namespace Robots
{
    [RequireComponent(typeof(Camera))]
    public class ImageModule : MonoBehaviour
    {
        [Header("Streaming")]
        public bool enableStreaming = true;
        public int targetFps = 10;
        public string imageIdentifier = "camera_id";

        [Header("Resolution")]
        public int width = 1280;
        public int height = 720;

        [Header("JPEG Settings")]
        [Range(1, 100)]
        public int jpegQuality = 70;

        [Header("FPS Logging")]
        public float fpsLogInterval = 2f;

        private Camera _camera;

        private RenderTexture[] _renderTextures;
        private int _activeIndex;
        private bool _readbackPending;

        private float _nextFrameTime;

        private void Start()
        {
            _camera = GetComponent<Camera>();

            _renderTextures = new RenderTexture[2];
            for (var i = 0; i < 2; i++)
            {
                _renderTextures[i] = new RenderTexture(width, height, 24)
                {
                    antiAliasing = 2,
                    filterMode = FilterMode.Point
                };
                _renderTextures[i].Create();
            }

            _camera.targetTexture = _renderTextures[_activeIndex];
        }

        private void LateUpdate()
        {
            if (!enableStreaming) return;
            if (Time.time < _nextFrameTime) return;
            if (_readbackPending) return;

            _nextFrameTime = Time.time + (1f / targetFps);

            AsyncGPUReadback.Request(_renderTextures[_activeIndex], 0, TextureFormat.RGB24, OnReadbackComplete);
            _readbackPending = true;

            _activeIndex = 1 - _activeIndex;
            _camera.targetTexture = _renderTextures[_activeIndex];
        }

        private void OnReadbackComplete(AsyncGPUReadbackRequest request)
        {
            _readbackPending = false;

            if (request.hasError || !enableStreaming) return;

            var rawSize = width * height * 3;
            var rawBuffer = ArrayPool<byte>.Shared.Rent(rawSize);
            request.GetData<byte>().CopyTo(rawBuffer);

            var captureTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var w = width;
            var h = height;
            var quality = jpegQuality;
            var id = imageIdentifier;

            Task.Run(() =>
            {
                try { EncodeAndSend(rawBuffer, rawSize, captureTime, w, h, quality, id); }
                finally { ArrayPool<byte>.Shared.Return(rawBuffer); }
            });
        }

        private static void EncodeAndSend(
            byte[] raw, int rawSize,
            ulong timestamp,
            int w, int h,
            int quality, string id)
        {
            var flipped = ArrayPool<byte>.Shared.Rent(rawSize);
            try
            {
                FlipVertical(raw, flipped, w, h);
                var jpegBytes = TurboJpeg.Encode(flipped, w, h, quality);

                var builder = new FlatBufferBuilder(1024 + jpegBytes.Length);
                var encoding = builder.CreateString("jpeg");
                var identifier = builder.CreateString(id);
                var jpegOffset = ImageFrame.CreateImageDataVector(builder, jpegBytes);
                var imageOffset = ImageFrame.CreateImageFrame(
                    builder, timestamp, 0, (uint)w, (uint)h,
                    encoding, identifier, jpegOffset
                );
                builder.Finish(imageOffset.Value);

                var wrapper = MessageWrapper.From(MessageType.ImageFrame, builder.SizedByteArray());
                SusConnection.Instance.Broadcast(wrapper);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(flipped);
            }
        }

        private static void FlipVertical(byte[] src, byte[] dst, int w, int h)
        {
            var stride = w * 3;
            for (var y = 0; y < h; y++)
            {
                src.AsSpan((h - 1 - y) * stride, stride).CopyTo(dst.AsSpan(y * stride, stride));
            }
        }

        private void OnDestroy()
        {
            if (_camera != null) _camera.targetTexture = null;

            if (_renderTextures != null)
            {
                foreach (var rt in _renderTextures)
                {
                    rt?.Release();
                }
            }
        }
    }
}