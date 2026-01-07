using Google.FlatBuffers;
using SUS;
using SUS.FlatBuffers;
using UnityEngine;

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
        public int width = 640;
        public int height = 480;

        [Header("JPEG Settings")]
        [Range(1, 100)]
        public int jpegQuality = 70;

        private Camera _camera;
        private RenderTexture _renderTexture;
        private Texture2D _readTexture;
        private float _nextFrameTime;

        private void Start()
        {
            _camera = GetComponent<Camera>();

            _renderTexture = new RenderTexture(
                width,
                height,
                24
            )
            {
                antiAliasing = 2,
                filterMode = FilterMode.Point
            };
            _renderTexture.Create();

            _camera.targetTexture = _renderTexture;

            _readTexture = new Texture2D(
                width,
                height,
                TextureFormat.RGB24,
                false
            );
        }

        private void LateUpdate()
        {
            if (!enableStreaming)
            {
                return;
            }

            if (Time.time < _nextFrameTime)
            {
                return;
            }

            _nextFrameTime = Time.time + (1f / targetFps);
            CaptureAndSend();
        }

        private void CaptureAndSend()
        {
            RenderTexture.active = _renderTexture;

            _readTexture.ReadPixels(
                new Rect(0, 0, width, height),
                0,
                0,
                false
            );

            _readTexture.Apply(false);

            RenderTexture.active = null;

            var jpegBytes = _readTexture.EncodeToJPG(jpegQuality);

            var builder = new FlatBufferBuilder(1024);
            var encoding = builder.CreateString("jpeg");
            var identifier = builder.CreateString(imageIdentifier);
            var jpegOffset = ImageFrame.CreateImageDataVector(builder, jpegBytes);

            var imageOffset = ImageFrame.CreateImageFrame(
                builder,
                (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                0,
                (uint)width,
                (uint)height,
                encoding,
                identifier,
                jpegOffset
            );
            builder.Finish(imageOffset.Value);

            var wrapper = FlatBufferUtils.FlatBufferWrapper.Create(FlatBufferUtils.FlatBufferType.ImageFrame, builder.SizedByteArray());
            SusConnection.Instance.Broadcast(wrapper.ToByteArray());
        }

        private void OnDestroy()
        {
            if (_camera != null)
            {
                _camera.targetTexture = null;
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
            }
        }
    }
}
