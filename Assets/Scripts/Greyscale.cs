using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Greyscale : ScriptableRendererFeature
{
    GreyscaleRenderPass pass;

    public override void Create()
    {
        var shader = Shader.Find("Recall/Greyscale");

        if (shader == null)
        {
            Debug.LogError("Cannot find shader: \"Recall/Greyscale\".");
            return;
        }

        var material = new Material(shader);

        pass = new GreyscaleRenderPass(material);
        name = "Greyscale";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var settings = VolumeManager.instance.stack.GetComponent<GreyscaleSettings>();

        if (settings != null && settings.IsActive())
        {
            pass.ConfigureInput(ScriptableRenderPassInput.Depth);
            //pass.ConfigureInput(ScriptableRenderPassInput.Normal);
            renderer.EnqueuePass(pass);
        }

    }

    protected override void Dispose(bool disposing)
    {
        pass.Dispose();
        base.Dispose(disposing);
    }

    class GreyscaleRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle tempTexHandle;
        private RTHandle tempDepthHandle;

        private RTHandle maskedObjectsHandle;

        public GreyscaleRenderPass(Material material)
        {
            this.material = material;

            profilingSampler = new ProfilingSampler("Greyscale");
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        private static RenderTextureDescriptor GetCopyPassDescriptor(RenderTextureDescriptor descriptor)
        {
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = (int)DepthBits.None;

            return descriptor;
        }

        private static RenderTextureDescriptor GetCopyPassDepthDescriptor(RenderTextureDescriptor descriptor)
        {
            descriptor.msaaSamples = 1;
            descriptor.depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
            descriptor.stencilFormat = GraphicsFormat.R8_UInt;
            descriptor.depthBufferBits = (int)DepthBits.Depth32;

            return descriptor;
        }

        public static bool CreateHandleWithDepthStencil(
            ref RTHandle handle,
            in RenderTextureDescriptor descriptor,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            bool isShadowMap = false,
            int anisoLevel = 1,
            float mipMapBias = 0,
            string name = "")
        {
            handle?.Release();
            handle = CustomRTHandles.Alloc(descriptor, filterMode, wrapMode, isShadowMap, anisoLevel, mipMapBias, name);
            return true;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ResetTarget();

            var tempTexDescriptor = cameraTextureDescriptor;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexHandle, GetCopyPassDescriptor(tempTexDescriptor));
            //RenderingUtils.ReAllocateIfNeeded(ref tempDepthHandle, GetCopyPassDepthDescriptor(tempTexDescriptor));

            //CreateHandleWithDepthStencil(ref tempTexHandle, GetCopyPassDescriptor(tempTexDescriptor));
            //CreateHandleWithDepthStencil(ref tempDepthHandle, GetCopyPassDepthDescriptor(tempTexDescriptor));

            RenderingUtils.ReAllocateIfNeeded(ref maskedObjectsHandle, GetCopyPassDescriptor(tempTexDescriptor));

            base.Configure(cmd, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            // Set Greyscale effect properties.
            var settings = VolumeManager.instance.stack.GetComponent<GreyscaleSettings>();
            material.SetFloat("_Strength", settings.strength.value);

            RTHandle cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
            //RTHandle cameraDepthHandle = renderingData.cameraData.renderer.cameraDepthTargetHandle;

            // Perform the Blit operations for the Greyscale effect.
            using (new ProfilingScope(cmd, profilingSampler))
            {
                //Blit(cmd, cameraTargetHandle, tempTexHandle, material, 0);
                //Blit(cmd, tempTexHandle, cameraTargetHandle);




                // Maybe we can do something here where we directly try to DrawRenderers
                // for only the recall objects directly into a new texture?
                // We can clear the texture then draw the objects in pure white.



                // Aaaaaalso, what if we do this in Unity 2021? Does it just work?

                CoreUtils.SetRenderTarget(cmd, maskedObjectsHandle);
                CoreUtils.ClearRenderTarget(cmd, ClearFlag.All, Color.black);

                var camera = renderingData.cameraData.camera;
                camera.TryGetCullingParameters(out var cullingParameters);
                var cullingResults = context.Cull(ref cullingParameters);

                ShaderTagId shaderTagId = new ShaderTagId("UniversalForward");

                var sortingSettings = new SortingSettings(camera);
                DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings)
                {
                    overrideShader = Shader.Find("Recall/MaskObject")
                };

                FilteringSettings filteringSettings = FilteringSettings.defaultValue;
                filteringSettings.layerMask = settings.objectMask.value;

                RendererListParams rendererParams = new RendererListParams(cullingResults, drawingSettings, filteringSettings);
                RendererList rendererList = context.CreateRendererList(ref rendererParams);

                cmd.DrawRendererList(rendererList);

                //context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
                //context.Submit();

                material.SetTexture("_MaskedObjects", maskedObjectsHandle);

                CoreUtils.SetRenderTarget(cmd, tempTexHandle);
                Blitter.BlitTexture(cmd, cameraTargetHandle, new Vector4(1, 1, 0, 0), 0, false);

                CoreUtils.SetRenderTarget(cmd, cameraTargetHandle);
                Blitter.BlitTexture(cmd, tempTexHandle, new Vector4(1, 1, 0, 0), material, 0);








                /*
                Debug.Log("cameraDepthHandle stencil format: " + cameraDepthHandle.rt.stencilFormat);
                Debug.Log("Temp depth handle stencil format: " + tempDepthHandle.rt.stencilFormat);

                CoreUtils.SetRenderTarget(cmd, tempTexHandle);
                Blitter.BlitTexture(cmd, cameraTargetHandle, new Vector4(1, 1, 0, 0), 0, false);

                CoreUtils.SetRenderTarget(cmd, tempDepthHandle);
                Blitter.BlitTexture(cmd, cameraDepthHandle, new Vector4(1, 1, 0, 0), 0, false);

                material.SetTexture("_DepthTexture", tempDepthHandle, RenderTextureSubElement.Stencil);

                CoreUtils.SetRenderTarget(cmd, cameraTargetHandle);
                Blitter.BlitTexture(cmd, tempTexHandle, new Vector4(1, 1, 0, 0), material, 0);
                */











                /*
                CoreUtils.SetRenderTarget(cmd, tempTexHandle);
                Blitter.BlitColorAndDepth(cmd, cameraTargetHandle, cameraDepthHandle.rt, new Vector4(1, 1, 0, 0), 0, true);

                CoreUtils.SetRenderTarget(cmd, cameraTargetHandle);
                Blitter.BlitTexture(cmd, tempDepthHandle, new Vector4(1, 1, 0, 0), material, 0);
                */





                /*
                CoreUtils.SetRenderTarget(cmd, tempTexHandle);
                Blitter.BlitTexture(cmd, cameraTargetHandle, new Vector4(1, 1, 0, 0), 0, false);

                CoreUtils.SetRenderTarget(cmd, cameraTargetHandle);
                Blitter.BlitTexture(cmd, tempTexHandle, new Vector4(1, 1, 0, 0), material, 0);
                */
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            tempTexHandle?.Release();
        }
    }
}
