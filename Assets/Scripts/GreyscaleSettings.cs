using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Recall/Greyscale")]
public sealed class GreyscaleSettings : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Greyscale effect intensity.")]
    public ClampedFloatParameter strength = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);

    [Tooltip("Apply to layers except these ones.")]
    public LayerMaskParameter objectMask = new LayerMaskParameter(0);

    public bool IsActive()
    {
        return strength.value > 0.0f && active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
