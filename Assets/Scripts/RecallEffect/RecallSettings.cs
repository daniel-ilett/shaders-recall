﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Recall/Greyscale")]
public sealed class RecallSettings : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Greyscale effect intensity.")]
    public ClampedFloatParameter strength = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);

    [Tooltip("Apply to layers except these ones.")]
    public LayerMaskParameter objectMask = new LayerMaskParameter(0);

    [Tooltip("Origin point of the scene wipe animation.")]
    public Vector2Parameter wipeOriginPoint = new Vector2Parameter(Vector3.zero);

    [Tooltip("Extent of the scene wipe animation.")]
    public ClampedFloatParameter wipeSize = new ClampedFloatParameter(0.25f, 0.0f, 5.0f);

    [Tooltip("Thickness of the screen wipe boundary.")]
    public ClampedFloatParameter wipeThickness = new ClampedFloatParameter(0.0f, 0.0f, 0.05f);

    [Tooltip("Noise scale for the screen wipe effect.")]
    public ClampedFloatParameter noiseScale = new ClampedFloatParameter(100.0f, 1.0f, 200.0f);

    [Tooltip("Noise strength for the screen wipe effect.")]
    public ClampedFloatParameter noiseStrength = new ClampedFloatParameter(0.1f, 0.0f, 1.0f);

    [Tooltip("Color of the boundary edges.")]
    public ColorParameter edgeColor = new ColorParameter(Color.yellow);

    public bool IsActive()
    {
        return strength.value > 0.0f && active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
