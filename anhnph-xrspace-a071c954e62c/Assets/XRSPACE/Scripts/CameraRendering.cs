using UnityEngine;
using XRSpace.Platform.VRcore;

public class CameraRendering : MonoBehaviour
{
#if UNITY_2018
    void OnEnable()
    {
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset)
        {
            UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering += beginCameraRendering;
        }
    }

    void OnDisable()
    {
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset)
        {
            UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering -= beginCameraRendering;
        }
    }

    void beginCameraRendering(Camera camera)
    {
        XRManager.Instance.SendMessage("OnCameraPreRender", camera);
    }
#elif UNITY_2019 || UNITY_2020
    void OnEnable()
    {
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset)
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += beginCameraRendering;
        }
    }

    void OnDisable()
    {
        if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset)
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= beginCameraRendering;
        }
    }

    void beginCameraRendering(UnityEngine.Rendering.ScriptableRenderContext context, Camera camera)
    {
        XRManager.Instance.SendMessage("OnCameraPreRender", camera);
    }
#endif
}