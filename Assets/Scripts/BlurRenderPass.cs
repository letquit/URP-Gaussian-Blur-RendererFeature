using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class BlurRenderPass : ScriptableRenderPass
{
    private static readonly int GridSize = Shader.PropertyToID("_GridSize");
    private static readonly int Spread = Shader.PropertyToID("_Spread");
    private readonly Material material;

    public BlurRenderPass(Material material)
    {
        this.material = material;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        requiresIntermediateTexture = true;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var resources = frameData.Get<UniversalResourceData>();
        var settings = VolumeManager.instance.stack.GetComponent<BlurSettings>();
        if (resources.isActiveTargetBackBuffer || settings == null || !settings.IsActive())
            return;

        TextureHandle source = resources.activeColorTexture;
        var desc = renderGraph.GetTextureDesc(source);
        desc.name = "_GaussianBlurIntermediate";
        desc.clearBuffer = false;
        desc.msaaSamples = MSAASamples.None;
        desc.bindTextureMS = false;
        TextureHandle intermediate = renderGraph.CreateTexture(desc);

        float strength = settings.strength.value;
        material.SetInteger(GridSize, Mathf.CeilToInt(strength * 6f) | 1);
        material.SetFloat(Spread, strength);

        // Write back to the original camera target after sampling the temporary texture.
        var horizontal = new RenderGraphUtils.BlitMaterialParameters(source, intermediate, material, 0)
        {
            geometry = RenderGraphUtils.FullScreenGeometryType.ProceduralTriangle
        };
        var vertical = new RenderGraphUtils.BlitMaterialParameters(intermediate, source, material, 1)
        {
            geometry = RenderGraphUtils.FullScreenGeometryType.ProceduralTriangle
        };
        renderGraph.AddBlitPass(horizontal, passName: "Gaussian Blur Horizontal");
        renderGraph.AddBlitPass(vertical, passName: "Gaussian Blur Vertical");
    }
}
