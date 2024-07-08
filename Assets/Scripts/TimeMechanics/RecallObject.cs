using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallObject : MonoBehaviour
{
    private static int maxFrameCount = 500;

    private List<RecallFrame> frames = new List<RecallFrame>();
    private RecallState recallState = RecallState.Advance;

    private new Rigidbody rigidbody = null;

    public event EventHandler RecallBufferExpired;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        switch(recallState)
        {
            case RecallState.Advance:
                {
                    AdvanceFrame();
                    break;
                }
            case RecallState.Recall:
                {
                    RecallFrame();
                    break;
                }
            case RecallState.Pause:
                {
                    Pause();
                    break;
                }
        }
    }

    private void AdvanceFrame()
    {
        RecallFrame frame = new RecallFrame(transform.position, transform.rotation);

        if(frames.Count == maxFrameCount)
        {
            frames.RemoveAt(0);
        }

        frames.Add(frame);
    }

    private void RecallFrame()
    {
        int frameIndex = frames.Count - 1;
        RecallFrame frame = frames[frameIndex];

        transform.position = frame.position;
        transform.rotation = frame.rotation;

        frames.RemoveAt(frameIndex);

        if(frames.Count == 0)
        {
            RecallBufferExpired?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Pause()
    {
        /*
        RecallFrame lastFrame = frames[frames.Count - 1];

        transform.position = lastFrame.position;
        transform.rotation = lastFrame.rotation;
        */
    }

    public void ChangeState(RecallState recallState)
    {
        this.recallState = recallState;

        switch (recallState)
        {
            case RecallState.Advance:
                {
                    if(rigidbody != null)
                    {
                        rigidbody.isKinematic = false;
                        rigidbody.velocity = Vector3.zero;
                        rigidbody.angularVelocity = Vector3.zero;
                    }

                    break;
                }
            case RecallState.Recall:
            case RecallState.Pause:
                {
                    if (rigidbody != null)
                    {
                        rigidbody.isKinematic = true;
                    }

                    break;
                }
        }
    }

    public float GetFramePercentage()
    {
        return frames.Count / (float)maxFrameCount;
    }
}
