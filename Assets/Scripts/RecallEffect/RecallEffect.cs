using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RecallEffect : ScriptableRendererFeature
{
    RecallRenderPass pass;

    public override void Create()
    {
        pass = new RecallRenderPass();
        name = "Recall";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var settings = VolumeManager.instance.stack.GetComponent<RecallSettings>();

        if (settings != null && settings.IsActive())
        {
            pass.ConfigureInput(ScriptableRenderPassInput.Depth);
            renderer.EnqueuePass(pass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        pass.Dispose();
        base.Dispose(disposing);
    }

    class RecallRenderPass : ScriptableRenderPass
    {
        private Material material;
        private Material maskMaterial;
        private RTHandle tempTexHandle;

        private RTHandle maskedObjectsHandle;

        public RecallRenderPass()
        {
            profilingSampler = new ProfilingSampler("Recall");
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        private void CreateMaterials()
        {
            var shader = Shader.Find("DanielIlett/Recall");

            if (shader == null)
            {
                Debug.LogError("Cannot find shader: \"DanielIlett/Recall\".");
                return;
            }

            material = new Material(shader);

            shader = Shader.Find("DanielIlett/MaskObject");

            if (shader == null)
            {
                Debug.LogError("Cannot find shader: \"DanielIlett/MaskObject\".");
                return;
            }

            maskMaterial = new Material(shader);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ResetTarget();

            var descriptor = cameraTextureDescriptor;

            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = (int)DepthBits.None;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexHandle, descriptor);

            descriptor.colorFormat = RenderTextureFormat.R8;
            RenderingUtils.ReAllocateIfNeeded(ref maskedObjectsHandle, descriptor);

            base.Configure(cmd, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera)
            {
                return;
            }

            if(material == null || maskMaterial == null)
            {
                CreateMaterials(); 
            }

            CommandBuffer cmd = CommandBufferPool.Get();

            // Set Recall effect properties.
            var settings = VolumeManager.instance.stack.GetComponent<RecallSettings>();
            material.SetFloat("_Strength", settings.strength.value);
            material.SetVector("_WipeOriginPoint", settings.wipeOriginPoint.value);
            material.SetFloat("_WipeSize", settings.wipeSize.value);
            material.SetFloat("_WipeThickness", settings.wipeThickness.value);
            material.SetFloat("_NoiseScale", settings.noiseScale.value);
            material.SetFloat("_NoiseStrength", settings.noiseStrength.value);
            material.SetFloat("_HighlightSize", settings.highlightSize.value);
            material.SetVector("_HighlightStrength", settings.highlightStrength.value);
            material.SetFloat("_HighlightSpeed", settings.highlightSpeed.value);
            material.SetVector("_HighlightThresholds", settings.highlightThresholds.value);
            material.SetColor("_EdgeColor", settings.edgeColor.value);

            RTHandle cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            
            // Perform the Blit operations for the Recall effect.
            using (new ProfilingScope(cmd, profilingSampler))
            {
                CoreUtils.SetRenderTarget(cmd, maskedObjectsHandle);
                CoreUtils.ClearRenderTarget(cmd, ClearFlag.All, Color.black);

                var camera = renderingData.cameraData.camera;
                camera.TryGetCullingParameters(out var cullingParameters);
                var cullingResults = context.Cull(ref cullingParameters);

                var sortingSettings = new SortingSettings(camera);

                FilteringSettings filteringSettings = 
                    new FilteringSettings(RenderQueueRange.all, settings.objectMask.value);

                ShaderTagId shaderTagId = new ShaderTagId("UniversalForward");

                DrawingSettings drawingSettingsLit = new DrawingSettings(shaderTagId, sortingSettings)
                {
                    overrideMaterial = maskMaterial
                };

                RendererListParams rendererParams = new RendererListParams(cullingResults, drawingSettingsLit, filteringSettings);
                RendererList rendererList = context.CreateRendererList(ref rendererParams);

                cmd.DrawRendererList(rendererList);

                shaderTagId = new ShaderTagId("SRPDefaultUnlit");

                DrawingSettings drawingSettingsUnlit = new DrawingSettings(shaderTagId, sortingSettings)
                {
                    overrideMaterial = maskMaterial
                };

                rendererParams = new RendererListParams(cullingResults, drawingSettingsUnlit, filteringSettings);
                rendererList = context.CreateRendererList(ref rendererParams);

                cmd.DrawRendererList(rendererList);

                material.SetTexture("_MaskedObjects", maskedObjectsHandle);

                Blitter.BlitCameraTexture(cmd, cameraTargetHandle, tempTexHandle);
                Blitter.BlitCameraTexture(cmd, tempTexHandle, cameraTargetHandle, material, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            tempTexHandle?.Release();
            maskedObjectsHandle?.Release();
        }
    }
}
