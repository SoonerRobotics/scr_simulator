using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ZED2iCameraLinker : MonoBehaviour
{
    public UniversalRendererData rendererData;
    public Camera depthCamera;

    void Awake()
    {
        if (rendererData == null)
        {
            Debug.LogError("ZED2iCameraLinker: rendererData is not assigned in the Inspector.");
            return;
        }

        if (depthCamera == null)
        {
            Debug.LogError("ZED2iCameraLinker: depthCamera is not assigned in the Inspector.");
            return;
        }

        if (rendererData.rendererFeatures == null || rendererData.rendererFeatures.Count == 0)
        {
            Debug.LogError("ZED2iCameraLinker: No renderer features found on the assigned rendererData.");
            return;
        }

        bool found = false;
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature is ZED2iDepthFeature depthFeature)
            {
                depthFeature.targetCamera = depthCamera;
                Debug.Log($"ZED2iCameraLinker: Linked '{depthCamera.name}' to ZED2iDepthFeature.");
                found = true;
                break;
            }
        }

        if (!found)
            Debug.LogError("ZED2iCameraLinker: ZED2iDepthFeature not found on the assigned rendererData.");
    }
}