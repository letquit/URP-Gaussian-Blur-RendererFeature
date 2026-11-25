using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurRendererFeature : ScriptableRendererFeature
{
    private Material material;
    private BlurRenderPass blurRenderPass;

    public override void Create()
    {
        Dispose(true);
        var shader = Resources.Load<Shader>("Blur");
        if (shader == null)
        {
            Debug.LogWarning("Blur Renderer Feature: Assets/Resources/Blur.shader was not found.");
            return;
        }

        material = CoreUtils.CreateEngineMaterial(shader);
        blurRenderPass = new BlurRenderPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (blurRenderPass == null || !renderingData.cameraData.postProcessEnabled)
            return;

        var settings = VolumeManager.instance.stack.GetComponent<BlurSettings>();
        if (settings != null && settings.IsActive())
            renderer.EnqueuePass(blurRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
        material = null;
        blurRenderPass = null;
    }
}
