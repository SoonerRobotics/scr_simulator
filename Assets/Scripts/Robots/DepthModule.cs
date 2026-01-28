using Google.FlatBuffers;
using Messages;
using SUS;
using UnityEngine;

namespace Robots
{
    [RequireComponent(typeof(Camera))]
    public class DepthModule : MonoBehaviour
    {
        [Header("Streaming")]
        public bool enableStreaming = true;
        public int targetFps = 10;
        public string imageIdentifier = "camera_id";

        [Header("Resolution")]
        public int width = 640;
        public int height = 480;

        [Header("Raycast Settings")]
        [Tooltip("How many pixels will a single raycast be assigned to. Defaults to 9.")]
        public int raycastResolution = 9;
        
        [Tooltip("The max distance of the raycast. Defaults to 5 meters.")]
        public float maxRaycastDistance = 5f;

        [Tooltip("The conversion factor from meters. Defaults to 1000 (millimeters)")]
        public float conversionFactor = 1000f;

        private Camera _camera;
        private Texture2D _depthTexture;
        private float _nextFrameTime;

        private void Start()
        {
            _camera = GetComponent<Camera>();
            _depthTexture = new Texture2D(
                width,
                height,
                TextureFormat.RFloat,
                false
            );
        }

        private void LateUpdate()
        {
            if (!enableStreaming || Time.time < _nextFrameTime)
            {
                return;
            }

            _nextFrameTime = Time.time + (1f / targetFps);
            CaptureAndSend();
        }

        private void CaptureAndSend()
        {
            // Generate and convert
            GenerateDepthImage();
            var depthBytes = ConvertDepthTextureToZ16(_depthTexture);

            // Construct the DepthFrame
            var builder = new FlatBufferBuilder(1024);
            var encoding = builder.CreateString("Z16");
            var identifier = builder.CreateString(imageIdentifier);
            var dataOffset = DepthFrame.CreateDepthDataVector(builder, depthBytes);
            var depthOffset = DepthFrame.CreateDepthFrame(
                builder,
                (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                0,
                (uint)width,
                (uint)height,
                encoding,
                identifier,
                dataOffset
            );
            builder.Finish(depthOffset.Value);

            // Send the frame over the network
            var wrapper = MessageWrapper.From(MessageType.DepthFrame, builder.SizedByteArray());
            SusConnection.Instance.Broadcast(wrapper);
        }

        public void GenerateDepthImage()
        {
            int step = resolutionBlockSize;
            for (int y = 0; y < imageHeight; y += step)
            {
                for (int x = 0; x < imageWidth; x += step)
                {
                    float depth = SampleDepthAtPixel(x, y);
                    for (int by = 0; by < step; by++)
                    {
                        for (int bx = 0; bx < step; bx++)
                        {
                            int px = x + bx;
                            int py = y + by;

                            if (px < imageWidth && py < imageHeight)
                            {
                                depthTexture.SetPixel(px, py, new Color(depth, 0, 0, 1));
                            }
                        }
                    }
                }
            }

            depthTexture.Apply(false);
        }

        public byte[] ConvertDepthTextureToZ16(Texture2D depthTexture)
        {
            var width = depthTexture.width;
            var height = depthTexture.height;
            var depthMeters = depthTexture.GetRawTextureData<float>();
            byte[] z16 = new byte[width * height * 2];

            int byteIndex = 0;
            for (int i = 0; i < depthMeters.Length; i++)
            {
                ushort depthValue = (ushort)Mathf.Clamp(depthMeters[i], 0f, 65535f);

                // pack into bytes (little endian)
                z16[byteIndex++] = (byte)(depthValue & 0xFF);
                z16[byteIndex++] = (byte)((depthValue >> 8) & 0xFF);
            }

            return z16;
        }

        float SampleDepthAtPixel(int x, int y)
        {
            float vx = (float)x / imageWidth;
            float vy = (float)y / imageHeight;

            Ray ray = cam.ViewportPointToRay(new Vector3(vx, vy, 0));

            int hitCount = Physics.RaycastNonAlloc(ray, hitBuffer, maxRaycastDistance);
            if (hitCount > 0)
            {
                return hitBuffer[0].distance * conversionFactor;
            }

            // Default to having hit nothing (e.g there are no objects in the distance)
            return 0f;
        }
    }
}
