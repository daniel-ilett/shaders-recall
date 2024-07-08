using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RecallFrame
{
    public Vector3 position;
    public Quaternion rotation;

    public RecallFrame(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}
