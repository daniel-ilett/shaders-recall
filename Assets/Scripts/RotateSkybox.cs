using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSkybox : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;

    private float startRotation;

    private void Start()
    {
        startRotation = RenderSettings.skybox.GetFloat("_Rotation");
    }

    private void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", startRotation + Time.time * rotateSpeed);
    }
}
