using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallObject : MonoBehaviour
{
    [SerializeField] private int defaultLayer;
    [SerializeField] private int recallLayer;

    private static int maxFrameCount = 500;

    private List<RecallFrame> frames = new List<RecallFrame>();
    private RecallState recallState = RecallState.Advance;

    private new Rigidbody rigidbody = null;

    public event RecallStartedEventHandler RecallStarted;
    public event RecalledFrameEventHandler RecalledFrame;
    public event EventHandler RecallEnded;
    public event EventHandler RecallBufferExpired;

    public delegate void RecallStartedEventHandler(object sender, List<RecallFrame> frames);
    public delegate void RecalledFrameEventHandler(object sender, RecallFrame frame);

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(rigidbody == null)
        {
            RunRecallState();
        }
    }

    private void FixedUpdate()
    {
        if (rigidbody != null)
        {
            RunRecallState();
        }
    }

    private void RunRecallState()
    {
        switch (recallState)
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
            case RecallState.EnterPause:
            case RecallState.ExitPause:
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

        RecalledFrame?.Invoke(this, frame);

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
                    gameObject.layer = defaultLayer;
                    if(rigidbody != null)
                    {
                        rigidbody.isKinematic = false;
                        rigidbody.velocity = Vector3.zero;
                        rigidbody.angularVelocity = Vector3.zero;
                    }

                    RecallEnded?.Invoke(this, EventArgs.Empty);

                    break;
                }
            case RecallState.Recall:
                {
                    break;
                }
            case RecallState.EnterPause:
                {
                    RecallStarted?.Invoke(this, frames);

                    gameObject.layer = recallLayer;

                    if (rigidbody != null)
                    {
                        rigidbody.isKinematic = true;
                    }

                    break;
                }
            case RecallState.ExitPause:
                {
                    gameObject.layer = defaultLayer;
                    break;
                }
        }
    }

    public float GetFramePercentage()
    {
        return frames.Count / (float)maxFrameCount;
    }
}
