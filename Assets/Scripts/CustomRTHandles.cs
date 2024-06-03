using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Reflection;

namespace UnityEngine.Rendering
{
    public class CustomRTHandles
    {
        /// <summary>
        /// Allocate a new fixed sized RTHandle with the default RTHandle System.
        /// </summary>
        /// <param name="descriptor">RenderTexture descriptor of the RTHandle.</param>
        /// <param name="filterMode">Filtering mode of the RTHandle.</param>
        /// <param name="wrapMode">Addressing mode of the RTHandle.</param>
        /// <param name="isShadowMap">Set to true if the depth buffer should be used as a shadow map.</param>
        /// <param name="anisoLevel">Anisotropic filtering level.</param>
        /// <param name="mipMapBias">Bias applied to mipmaps during filtering.</param>
        /// <param name="name">Name of the RTHandle.</param>
        /// <returns>A new RTHandle.</returns>
        public static RTHandle Alloc(
            in RenderTextureDescriptor descriptor,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            bool isShadowMap = false,
            int anisoLevel = 1,
            float mipMapBias = 0,
            string name = ""
        )
        {
            RTHandleSystem defaultRTSystem = (RTHandleSystem)(typeof(RTHandles).GetField("s_DefaultInstance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

            var result = Alloc(
                descriptor.width,
                descriptor.height,
                descriptor.volumeDepth,
                (DepthBits)descriptor.depthBufferBits,
                descriptor.graphicsFormat,
                filterMode,
                wrapMode,
                descriptor.dimension,
                descriptor.enableRandomWrite,
                descriptor.useMipMap,
                descriptor.autoGenerateMips,
                isShadowMap,
                anisoLevel,
                mipMapBias,
                (MSAASamples)descriptor.msaaSamples,
                descriptor.bindMS,
                descriptor.useDynamicScale,
                descriptor.memoryless,
                descriptor.vrUsage,
                name,
                descriptor.depthStencilFormat,
                descriptor.stencilFormat
            );
            return result;
        }

        /// <summary>
        /// Allocate a new fixed sized RTHandle.
        /// </summary>
        /// <param name="width">With of the RTHandle.</param>
        /// <param name="height">Heigh of the RTHandle.</param>
        /// <param name="slices">Number of slices of the RTHandle.</param>
        /// <param name="depthBufferBits">Bit depths of a depth buffer.</param>
        /// <param name="colorFormat">GraphicsFormat of a color buffer.</param>
        /// <param name="filterMode">Filtering mode of the RTHandle.</param>
        /// <param name="wrapMode">Addressing mode of the RTHandle.</param>
        /// <param name="dimension">Texture dimension of the RTHandle.</param>
        /// <param name="enableRandomWrite">Set to true to enable UAV random read writes on the texture.</param>
        /// <param name="useMipMap">Set to true if the texture should have mipmaps.</param>
        /// <param name="autoGenerateMips">Set to true to automatically generate mipmaps.</param>
        /// <param name="isShadowMap">Set to true if the depth buffer should be used as a shadow map.</param>
        /// <param name="anisoLevel">Anisotropic filtering level.</param>
        /// <param name="mipMapBias">Bias applied to mipmaps during filtering.</param>
        /// <param name="msaaSamples">Number of MSAA samples for the RTHandle.</param>
        /// <param name="bindTextureMS">Set to true if the texture needs to be bound as a multisampled texture in the shader.</param>
        /// <param name="useDynamicScale">Set to true to use hardware dynamic scaling.</param>
        /// <param name="memoryless">Use this property to set the render texture memoryless modes.</param>
        /// <param name="vrUsage">Special treatment of the VR eye texture used in stereoscopic rendering.</param>
        /// <param name="name">Name of the RTHandle.</param>
        /// <param name="depthStencilFormat">Format of the DepthStencil.</param>
        /// <param name="stencilFormat">Format of the Stencil.</param>
        /// <returns></returns>
        public static RTHandle Alloc(
            int width,
            int height,
            int slices = 1,
            DepthBits depthBufferBits = DepthBits.None,
            GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            TextureDimension dimension = TextureDimension.Tex2D,
            bool enableRandomWrite = false,
            bool useMipMap = false,
            bool autoGenerateMips = true,
            bool isShadowMap = false,
            int anisoLevel = 1,
            float mipMapBias = 0f,
            MSAASamples msaaSamples = MSAASamples.None,
            bool bindTextureMS = false,
            bool useDynamicScale = false,
            RenderTextureMemoryless memoryless = RenderTextureMemoryless.None,
            VRTextureUsage vrUsage = VRTextureUsage.None,
            string name = "",
            GraphicsFormat depthStencilFormat = GraphicsFormat.None,
            GraphicsFormat stencilFormat = GraphicsFormat.None
        )
        {
            return Alloc(width, height, wrapMode, wrapMode, wrapMode, slices, depthBufferBits, colorFormat, filterMode, dimension, enableRandomWrite, useMipMap,
                autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, memoryless, vrUsage, name, depthStencilFormat, stencilFormat);
        }

        /// <summary>
        /// Allocate a new fixed sized RTHandle.
        /// </summary>
        /// <param name="width">With of the RTHandle.</param>
        /// <param name="height">Heigh of the RTHandle.</param>
        /// <param name="wrapModeU">U coordinate wrapping mode of the RTHandle.</param>
        /// <param name="wrapModeV">V coordinate wrapping mode of the RTHandle.</param>
        /// <param name="wrapModeW">W coordinate wrapping mode of the RTHandle.</param>
        /// <param name="slices">Number of slices of the RTHandle.</param>
        /// <param name="depthBufferBits">Bit depths of a depth buffer.</param>
        /// <param name="colorFormat">GraphicsFormat of a color buffer.</param>
        /// <param name="filterMode">Filtering mode of the RTHandle.</param>
        /// <param name="dimension">Texture dimension of the RTHandle.</param>
        /// <param name="enableRandomWrite">Set to true to enable UAV random read writes on the texture.</param>
        /// <param name="useMipMap">Set to true if the texture should have mipmaps.</param>
        /// <param name="autoGenerateMips">Set to true to automatically generate mipmaps.</param>
        /// <param name="isShadowMap">Set to true if the depth buffer should be used as a shadow map.</param>
        /// <param name="anisoLevel">Anisotropic filtering level.</param>
        /// <param name="mipMapBias">Bias applied to mipmaps during filtering.</param>
        /// <param name="msaaSamples">Number of MSAA samples for the RTHandle.</param>
        /// <param name="bindTextureMS">Set to true if the texture needs to be bound as a multisampled texture in the shader.</param>
        /// <param name="useDynamicScale">Set to true to use hardware dynamic scaling.</param>
        /// <param name="memoryless">Use this property to set the render texture memoryless modes.</param>
        /// <param name="vrUsage">Special treatment of the VR eye texture used in stereoscopic rendering.</param>
        /// <param name="name">Name of the RTHandle.</param>
        /// <param name="depthStencilFormat">Format of the DepthStencil.</param>
        /// <param name="stencilFormat">Format of the Stencil.</param>
        /// <returns></returns>
        public static RTHandle Alloc(
            int width,
            int height,
            TextureWrapMode wrapModeU,
            TextureWrapMode wrapModeV,
            TextureWrapMode wrapModeW = TextureWrapMode.Repeat,
            int slices = 1,
            DepthBits depthBufferBits = DepthBits.None,
            GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB,
            FilterMode filterMode = FilterMode.Point,
            TextureDimension dimension = TextureDimension.Tex2D,
            bool enableRandomWrite = false,
            bool useMipMap = false,
            bool autoGenerateMips = true,
            bool isShadowMap = false,
            int anisoLevel = 1,
            float mipMapBias = 0f,
            MSAASamples msaaSamples = MSAASamples.None,
            bool bindTextureMS = false,
            bool useDynamicScale = false,
            RenderTextureMemoryless memoryless = RenderTextureMemoryless.None,
            VRTextureUsage vrUsage = VRTextureUsage.None,
            string name = "",
            GraphicsFormat depthStencilFormat = GraphicsFormat.None,
            GraphicsFormat stencilFormat = GraphicsFormat.None
        )
        {
            bool enableMSAA = msaaSamples != MSAASamples.None;
            if (!enableMSAA && bindTextureMS == true)
            {
                Debug.LogWarning("RTHandle allocated without MSAA but with bindMS set to true, forcing bindMS to false.");
                bindTextureMS = false;
            }

            // We need to handle this in an explicit way since GraphicsFormat does not expose depth formats. TODO: Get rid of this branch once GraphicsFormat'll expose depth related formats
            RenderTexture rt;
            if (isShadowMap || depthBufferBits != DepthBits.None)
            {
                RenderTextureFormat format = isShadowMap ? RenderTextureFormat.Shadowmap : RenderTextureFormat.Depth;
                rt = new RenderTexture(width, height, (int)depthBufferBits, format, RenderTextureReadWrite.Linear)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    volumeDepth = slices,
                    filterMode = filterMode,
                    wrapModeU = wrapModeU,
                    wrapModeV = wrapModeV,
                    wrapModeW = wrapModeW,
                    dimension = dimension,
                    enableRandomWrite = enableRandomWrite,
                    useMipMap = useMipMap,
                    autoGenerateMips = autoGenerateMips,
                    anisoLevel = anisoLevel,
                    mipMapBias = mipMapBias,
                    antiAliasing = (int)msaaSamples,
                    bindTextureMS = bindTextureMS,
                    useDynamicScale = DynamicResolutionHandler.instance.RequestsHardwareDynamicResolution() && useDynamicScale,
                    memorylessMode = memoryless,
                    vrUsage = vrUsage,
                    name = CoreUtils.GetRenderTargetAutoName(width, height, slices, format, name, mips: useMipMap, enableMSAA: enableMSAA, msaaSamples: msaaSamples),
                    depthStencilFormat = depthStencilFormat,
                    stencilFormat = stencilFormat
                };
            }
            else
            {
                rt = new RenderTexture(width, height, (int)depthBufferBits, colorFormat)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    volumeDepth = slices,
                    filterMode = filterMode,
                    wrapModeU = wrapModeU,
                    wrapModeV = wrapModeV,
                    wrapModeW = wrapModeW,
                    dimension = dimension,
                    enableRandomWrite = enableRandomWrite,
                    useMipMap = useMipMap,
                    autoGenerateMips = autoGenerateMips,
                    anisoLevel = anisoLevel,
                    mipMapBias = mipMapBias,
                    antiAliasing = (int)msaaSamples,
                    bindTextureMS = bindTextureMS,
                    useDynamicScale = DynamicResolutionHandler.instance.RequestsHardwareDynamicResolution() && useDynamicScale,
                    memorylessMode = memoryless,
                    vrUsage = vrUsage,
                    name = CoreUtils.GetRenderTargetAutoName(width, height, slices, colorFormat, dimension, name, mips: useMipMap, enableMSAA: enableMSAA, msaaSamples: msaaSamples, dynamicRes: useDynamicScale),
                    depthStencilFormat = depthStencilFormat,
                    stencilFormat = stencilFormat
                };
            }

            rt.Create();

            RTHandleSystem defaultRTSystem = (RTHandleSystem)(typeof(RTHandles).GetField("s_DefaultInstance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));

            //var newRT = new RTHandle(defaultRTSystem);
            var ctor = typeof(RTHandle).GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)[0];
            var newRT = ctor.Invoke(new object[1] { defaultRTSystem }) as RTHandle;

            //newRT.SetRenderTexture(rt);
            var setRenderTexture = typeof(RTHandle).GetMethod("SetRenderTexture", BindingFlags.NonPublic | BindingFlags.Instance);
            setRenderTexture.Invoke(newRT, new object[1] { rt });

            //newRT.useScaling = false;
            typeof(RTHandle).GetProperty("useScaling", BindingFlags.Public | BindingFlags.Instance).SetValue(newRT, false);

            //newRT.m_EnableRandomWrite = enableRandomWrite;
            typeof(RTHandle).GetField("m_EnableRandomWrite", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(newRT, enableRandomWrite);

            //newRT.m_EnableMSAA = enableMSAA;
            typeof(RTHandle).GetField("m_EnableMSAA", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(newRT, enableMSAA);

            //newRT.m_EnableHWDynamicScale = useDynamicScale;
            typeof(RTHandle).GetField("m_EnableHWDynamicScale", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(newRT, useDynamicScale);

            //newRT.m_Name = name;
            typeof(RTHandle).GetField("m_Name", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(newRT, name );

            //newRT.referenceSize = new Vector2Int(width, height);
            typeof(RTHandle).GetProperty("referenceSize", BindingFlags.Public | BindingFlags.Instance).SetValue(newRT, new Vector2Int(width, height));

            return newRT;
        }
    }
}
