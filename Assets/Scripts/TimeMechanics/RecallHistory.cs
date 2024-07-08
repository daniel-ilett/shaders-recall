using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallHistory : MonoBehaviour
{
    [SerializeField] private RecallObject parentObject;

    private LineRenderer lineRenderer;
    private int framesCounted = 0;

    private static int keyframeWidth = 10;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        parentObject.RecallStarted += StartRecall;
        parentObject.RecalledFrame += RecallFrame;
        parentObject.RecallEnded += EndRecall;

        gameObject.SetActive(false);
    }

    public void StartRecall(object sender, List<RecallFrame> frames)
    {
        gameObject.SetActive(true);

        List<Vector3> positions = new List<Vector3>();

        for(int i = 0; i < frames.Count; i += keyframeWidth)
        {
            positions.Add(frames[i].position);
        }

        if(frames.Count % keyframeWidth != 0)
        {
            positions.Add(frames[frames.Count - 1].position);
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());

        framesCounted = frames.Count;
    }

    public void RecallFrame(object sender, RecallFrame frame)
    {
        // If we are about to hit a keyframe, just remove the last keyframe.
        if(framesCounted % keyframeWidth == 1)
        {
            Vector3[] positions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(positions);
            
            Array.Resize(ref positions, positions.Length - 1);

            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }
        else
        {
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, frame.position);
        }

        framesCounted--;
    }

    public void EndRecall(object sender, EventArgs e)
    {
        gameObject.SetActive(false);
    }
}
