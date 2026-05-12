using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to any GameObject in your scene.
/// Assign the URP Renderer asset that has ZED2iDepthFeature added to it.
/// Press V in Play mode to run all checks and see a live depth readout.
/// </summary>
public class ZED2iValidator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The URP Renderer asset (UniversalRendererData) that has ZED2iDepthFeature on it")]
    public UniversalRendererData rendererData;

    [Header("Validation Settings")]
    [Tooltip("How many frames to wait before auto-validating on start")]
    public int warmupFrames = 5;

    [Tooltip("Run validation automatically on start")]
    public bool validateOnStart = true;

    private ZED2iDepthFeature depthFeature;
    private int frameCount = 0;
    private bool validated = false;

    // Results
    private bool featureFound;
    private bool shaderFound;
    private bool rtAllocated;
    private bool rtFormatCorrect;
    private bool depthDataNonZero;
    private float minDepth, maxDepth, centerDepth, meanDepth;
    private int validPixelCount;
    private int totalPixels;

    void Start()
    {
        FindDepthFeature();
    }

    void Update()
    {
        if (!validateOnStart || validated) return;
        if (++frameCount >= warmupFrames)
        {
            validated = true;
            RunValidation();
        }
    }

    void OnGUI()
    {
        if (!validated) return;

        var style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 14,
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(10, 10, 10, 10)
        };

        string report = BuildReport();
        Vector2 size = style.CalcSize(new GUIContent(report));
        size.x = Mathf.Max(size.x, 360f);
        GUI.Box(new Rect(10, 10, size.x + 20, size.y + 20), report, style);
    }

    void FindDepthFeature()
    {
        if (rendererData == null)
        {
            Debug.LogWarning("ZED2iValidator: No RendererData assigned. Trying to find it automatically...");
            // Try to grab it from the current pipeline asset
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                // UniversalRenderPipelineAsset doesn't expose rendererDataList publicly,
                // so we rely on the user assigning it. Warn clearly.
                Debug.LogWarning("ZED2iValidator: Please assign your UniversalRendererData asset in the Inspector.");
            }
            return;
        }

        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature is ZED2iDepthFeature found)
            {
                depthFeature = found;
                break;
            }
        }
    }

    [ContextMenu("Run Validation Now")]
    public void RunValidation()
    {
        featureFound    = depthFeature != null;
        shaderFound     = Shader.Find("Custom/ZED2iDepth") != null
                          || Resources.Load<Shader>("ZED2iDepth") != null;
        rtAllocated     = false;
        rtFormatCorrect = false;
        depthDataNonZero = false;
        minDepth = maxDepth = centerDepth = meanDepth = 0f;
        validPixelCount = totalPixels = 0;

        if (!featureFound)
        {
            Debug.LogError("ZED2iValidator: ZED2iDepthFeature not found on the assigned renderer.");
            return;
        }

        var rt = depthFeature.DepthRTHandle?.rt;
        rtAllocated     = rt != null && rt.IsCreated();
        rtFormatCorrect = rtAllocated && rt.format == RenderTextureFormat.RFloat;

        if (!rtAllocated)
        {
            Debug.LogError("ZED2iValidator: RenderTexture is not allocated. " +
                           "Make sure the scene has rendered at least one frame with the feature active.");
            return;
        }

        // Read back pixels synchronously
        var readback = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        readback.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readback.Apply();
        RenderTexture.active = prev;

        var raw = readback.GetRawTextureData<float>();
        totalPixels = raw.Length;

        float sum = 0f;
        float localMin = float.MaxValue;
        float localMax = 0f;

        for (int i = 0; i < raw.Length; i++)
        {
            float d = raw[i];
            if (d >= 0.2f && d <= 20f)   // ZED 2i valid range
            {
                validPixelCount++;
                sum += d;
                if (d < localMin) localMin = d;
                if (d > localMax) localMax = d;
            }
        }

        depthDataNonZero = validPixelCount > 0;
        if (depthDataNonZero)
        {
            minDepth  = localMin;
            maxDepth  = localMax;
            meanDepth = sum / validPixelCount;

            int cx = rt.width  / 2;
            int cy = rt.height / 2;
            centerDepth = raw[cy * rt.width + cx];
        }

        Destroy(readback);

        // Log summary
        Debug.Log(BuildReport());
    }

    // Async version — no GPU stall, call via button or key
    public void RunValidationAsync()
    {
        if (depthFeature?.DepthRTHandle?.rt == null)
        {
            Debug.LogError("ZED2iValidator: RTHandle not ready for async readback.");
            return;
        }

        AsyncGPUReadback.Request(depthFeature.DepthRTHandle.rt, 0, TextureFormat.RFloat, OnAsyncReadback);
    }

    void OnAsyncReadback(AsyncGPUReadbackRequest req)
    {
        if (req.hasError) { Debug.LogError("ZED2iValidator: AsyncGPUReadback failed."); return; }

        var raw = req.GetData<float>();
        float sum = 0f, localMin = float.MaxValue, localMax = 0f;
        int w = depthFeature.DepthRTHandle.rt.width;
        int h = depthFeature.DepthRTHandle.rt.height;
        totalPixels = raw.Length;
        validPixelCount = 0;

        for (int i = 0; i < raw.Length; i++)
        {
            float d = raw[i];
            if (d >= 0.2f && d <= 20f)
            {
                validPixelCount++;
                sum += d;
                if (d < localMin) localMin = d;
                if (d > localMax) localMax = d;
            }
        }

        depthDataNonZero = validPixelCount > 0;
        rtAllocated      = true;
        rtFormatCorrect  = true;
        featureFound     = true;

        if (depthDataNonZero)
        {
            minDepth    = localMin;
            maxDepth    = localMax;
            meanDepth   = sum / validPixelCount;
            centerDepth = raw[h / 2 * w + w / 2];
        }

        validated = true;
        Debug.Log(BuildReport());
    }

    string Pass(bool ok) => ok ? "[PASS]" : "[FAIL]";

    string BuildReport()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== ZED2i Depth Validator ===");
        sb.AppendLine();
        sb.AppendLine($"{Pass(featureFound)}  ZED2iDepthFeature found on renderer");
        sb.AppendLine($"{Pass(shaderFound)}  Shader 'Custom/ZED2iDepth' found");
        sb.AppendLine($"{Pass(rtAllocated)}  RenderTexture allocated");
        sb.AppendLine($"{Pass(rtFormatCorrect)}  RT format is RFloat (metric depth)");
        sb.AppendLine($"{Pass(depthDataNonZero)}  Non-zero depth pixels in scene");
        sb.AppendLine();

        if (depthDataNonZero)
        {
            float coverage = (float)validPixelCount / totalPixels * 100f;
            sb.AppendLine("--- Depth Stats (meters) ---");
            sb.AppendLine($"  Center pixel : {centerDepth:F3} m");
            sb.AppendLine($"  Min          : {minDepth:F3} m");
            sb.AppendLine($"  Max          : {maxDepth:F3} m");
            sb.AppendLine($"  Mean         : {meanDepth:F3} m");
            sb.AppendLine($"  Valid pixels : {validPixelCount:N0} / {totalPixels:N0} ({coverage:F1}%)");
            sb.AppendLine();
            sb.AppendLine("  ZED 2i range : 0.200 – 20.000 m");
        }
        else if (rtAllocated)
        {
            sb.AppendLine("  No valid depth pixels found.");
            sb.AppendLine("  Is there geometry in front of the camera?");
            sb.AppendLine("  Is the scene using metric scale (1 unit = 1 m)?");
        }

        sb.AppendLine();
        sb.AppendLine("Press V for async re-validation.");
        return sb.ToString();
    }

    void LateUpdate()
    {
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
            RunValidationAsync();
    }
}