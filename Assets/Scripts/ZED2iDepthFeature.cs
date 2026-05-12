using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class ZED2iDepthFeature : ScriptableRendererFeature
{
    [Header("Target Camera")]
    public Camera targetCamera;

    class PassData
    {
        public RendererListHandle rendererList;
    }

    class DepthPass : ScriptableRenderPass
    {
        private Material depthMat;
        private RTHandle depthRTHandle;
        private int width;
        private int height;

        public RTHandle DepthRTHandle => depthRTHandle;
        public Camera targetCamera;

        public DepthPass(int width, int height, Camera targetCamera)
        {
            this.width  = width;
            this.height = height;
            this.targetCamera = targetCamera;
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        private void EnsureResources()
        {
            if (depthMat == null)
            {
                var shader = Shader.Find("Custom/ZED2iDepth");
                if (shader == null)
                {
                    return;
                }
                depthMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            }

            var desc = new RenderTextureDescriptor(width, height, RenderTextureFormat.RFloat, 0);
            RenderingUtils.ReAllocateHandleIfNeeded(
                ref depthRTHandle,
                desc,
                FilterMode.Point,
                TextureWrapMode.Clamp,
                name: "_ZED2iDepthTexture"
            );
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            EnsureResources();

            if (depthMat == null || depthRTHandle == null) return;

            var renderingData = frameData.Get<UniversalRenderingData>();
            var cameraData    = frameData.Get<UniversalCameraData>();
            var lightData     = frameData.Get<UniversalLightData>();

            if (targetCamera != null && cameraData.camera != targetCamera)
            {
                return;
            }

            TextureHandle depthOutput = renderGraph.ImportTexture(depthRTHandle);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("ZED2i Depth Pass", out var passData))
            {
                var drawSettings = RenderingUtils.CreateDrawingSettings(
                    new ShaderTagId("UniversalForward"),
                    renderingData,
                    cameraData,
                    lightData,
                    cameraData.defaultOpaqueSortFlags
                );
                drawSettings.overrideMaterial      = depthMat;
                drawSettings.overrideMaterialPassIndex = 0;

                var filterSettings = new FilteringSettings(RenderQueueRange.opaque);

                passData.rendererList = renderGraph.CreateRendererList(
                    new RendererListParams(renderingData.cullResults, drawSettings, filterSettings)
                );

                builder.UseRendererList(passData.rendererList);
                builder.SetRenderAttachment(depthOutput, 0, AccessFlags.Write);

                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    ctx.cmd.ClearRenderTarget(false, true, Color.clear);
                    ctx.cmd.DrawRendererList(data.rendererList);
                });
            }
        }

        public void Dispose()
        {
            depthRTHandle?.Release();
            depthRTHandle = null;

            if (depthMat != null)
            {
                CoreUtils.Destroy(depthMat);
                depthMat = null;
            }
        }
    }

    public int width  = 1280;
    public int height = 720;

    private DepthPass depthPass;
    public RTHandle DepthRTHandle => depthPass?.DepthRTHandle;

    public override void Create()
    {
        depthPass = new DepthPass(width, height, targetCamera);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        depthPass.targetCamera = targetCamera;
        renderer.EnqueuePass(depthPass);
    }

    protected override void Dispose(bool disposing)
    {
        depthPass?.Dispose();
        depthPass = null;
    }
}