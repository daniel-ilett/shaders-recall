﻿using UnityEngine;
using UnityEngine.Experimental.Rendering;
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
            //pass.ConfigureInput(ScriptableRenderPassInput.Normal);
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
        private RTHandle tempTexHandle;
        //private RTHandle tempDepthHandle;

        private RTHandle maskedObjectsHandle;

        public RecallRenderPass()
        {
            profilingSampler = new ProfilingSampler("Recall");
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        private void CreateMaterial()
        {
            var shader = Shader.Find("DanielIlett/Recall");

            if (shader == null)
            {
                Debug.LogError("Cannot find shader: \"DanielIlett/Recall\".");
                return;
            }

            material = new Material(shader);
        }

        /*
        private static RenderTextureDescriptor GetCopyPassDescriptor(RenderTextureDescriptor descriptor)
        {
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = (int)DepthBits.None;

            return descriptor;
        }

        private static RenderTextureDescriptor GetMaskPassDescriptor(RenderTextureDescriptor descriptor)
        {
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = (int)DepthBits.None;
            descriptor.colorFormat = RenderTextureFormat.R8;

            return descriptor;
        }
        */

        /*
        private static RenderTextureDescriptor GetCopyPassDepthDescriptor(RenderTextureDescriptor descriptor)
        {
            descriptor.msaaSamples = 1;
            descriptor.depthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
            descriptor.stencilFormat = GraphicsFormat.R8_UInt;
            descriptor.depthBufferBits = (int)DepthBits.Depth32;

            return descriptor;
        }
        */

        /*
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
        */

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ResetTarget();

            var descriptor = cameraTextureDescriptor;

            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = (int)DepthBits.None;

            RenderingUtils.ReAllocateIfNeeded(ref tempTexHandle, descriptor);
            //RenderingUtils.ReAllocateIfNeeded(ref tempDepthHandle, GetCopyPassDepthDescriptor(tempTexDescriptor));

            //CreateHandleWithDepthStencil(ref tempTexHandle, GetCopyPassDescriptor(tempTexDescriptor));
            //CreateHandleWithDepthStencil(ref tempDepthHandle, GetCopyPassDepthDescriptor(tempTexDescriptor));

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

            if(material == null)
            {
                CreateMaterial(); 
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
            //RTHandle cameraDepthHandle = renderingData.cameraData.renderer.cameraDepthTargetHandle;

            // Perform the Blit operations for the Recall effect.
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




                var sortingSettings = new SortingSettings(camera);

                FilteringSettings filteringSettings = 
                    new FilteringSettings(RenderQueueRange.all, settings.objectMask.value);

                ShaderTagId shaderTagId = new ShaderTagId("UniversalForward");

                Material mat = new Material(Shader.Find("Recall/MaskObject"));
                //mat.SetTexture("_John", cameraDepthHandle);

                DrawingSettings drawingSettingsLit = new DrawingSettings(shaderTagId, sortingSettings)
                {
                    //overrideShader = Shader.Find("Recall/MaskObject")
                    overrideMaterial = mat
                };

                RendererListParams rendererParams = new RendererListParams(cullingResults, drawingSettingsLit, filteringSettings);
                RendererList rendererList = context.CreateRendererList(ref rendererParams);

                cmd.DrawRendererList(rendererList);














                shaderTagId = new ShaderTagId("SRPDefaultUnlit");

                DrawingSettings drawingSettingsUnlit = new DrawingSettings(shaderTagId, sortingSettings)
                {
                    overrideMaterial = mat
                };

                rendererParams = new RendererListParams(cullingResults, drawingSettingsUnlit, filteringSettings);
                rendererList = context.CreateRendererList(ref rendererParams);

                cmd.DrawRendererList(rendererList);













                // This didn't work, but I don't know why. Something to do with Submit I guess.
                //context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
                //context.Submit();

                material.SetTexture("_MaskedObjects", maskedObjectsHandle);

                Blitter.BlitCameraTexture(cmd, cameraTargetHandle, tempTexHandle);
                Blitter.BlitCameraTexture(cmd, tempTexHandle, cameraTargetHandle, material, 0);








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
            maskedObjectsHandle?.Release();
        }
    }
}
