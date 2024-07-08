using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RecallController : MonoBehaviour
{
    [SerializeField] private RecallObject testObject;
    [SerializeField] private Volume recallVolume;

    private RecallSettings recallEffect;
    private Camera mainCamera;

    private bool isRecallSelecting = false;
    private bool isRecalling = false;
    private Coroutine pauseRoutine = null;

    private void Start()
    {
        if(recallVolume != null)
        {
            recallVolume.profile.TryGet(out recallEffect);
        }

        recallEffect.active = false;
        recallEffect.wipeSize.overrideState = true;
        recallEffect.wipeOriginPoint.overrideState = true;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (pauseRoutine == null)
            {
                isRecalling = !isRecalling;

                if (isRecalling)
                {
                    recallEffect.active = true;
                    pauseRoutine = StartCoroutine(EnterPauseAnimation(RecallState.EnterPause, RecallState.Recall));
                    testObject.RecallBufferExpired += ExpireRecallMemory;
                }
                else
                {
                    recallEffect.active = false;
                    pauseRoutine = StartCoroutine(ExitPauseAnimation(RecallState.ExitPause, RecallState.Advance));
                    testObject.RecallBufferExpired -= ExpireRecallMemory;
                }
            }




            /*
            // Start looking for a recall object.
            if(!isRecallSelecting && !isRecalling && pauseRoutine == null)
            {
                isRecallSelecting = true;
                return;
            }

            // Try to start the recall.
            if(isRecallSelecting)
            {
                // Check whatever is in view of the player and recall it.
                bool canRecall = true;

                if(canRecall)
                {
                    recallEffect.active = true;
                    pauseRoutine = StartCoroutine(EnterPauseAnimation(RecallState.EnterPause, RecallState.Recall));
                    testObject.RecallBufferExpired += ExpireRecallMemory;
                    isRecalling = true;
                    isRecallSelecting = false;
                }

                return;
            }

            // Cancel the recall.
            if (isRecalling)
            {
                recallEffect.active = false;
                pauseRoutine = StartCoroutine(ExitPauseAnimation(RecallState.ExitPause, RecallState.Advance));
                testObject.RecallBufferExpired -= ExpireRecallMemory;
                isRecalling = false;
            }
            */
        }
    }

    public void ExpireRecallMemory(object sender, EventArgs e)
    {
        recallEffect.active = false;
        isRecalling = false;
        pauseRoutine = StartCoroutine(ExitPauseAnimation(RecallState.ExitPause, RecallState.Advance));
        testObject.RecallBufferExpired -= ExpireRecallMemory;
    }

    private IEnumerator EnterPauseAnimation(RecallState from, RecallState to)
    {
        testObject.ChangeState(from);
        var wait = new WaitForEndOfFrame();

        float t;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        for(t = 0.0f; t < 0.5f; t += Time.deltaTime)
        {
            recallEffect.wipeSize.value = 0.0f;
            //recallEffect.wipeSize.value = Mathf.Lerp(0.0f, 0.25f, t / 0.5f);
            recallEffect.wipeOriginPoint.value = mainCamera.WorldToScreenPoint(testObject.transform.position) / screenSize;
            yield return wait;
        }

        for (t = 0.0f; t < 1.0f; t += Time.deltaTime)
        {
            recallEffect.wipeSize.value = Mathf.Lerp(0.0f, 5.0f, t * t * t);
            recallEffect.wipeOriginPoint.value = mainCamera.WorldToScreenPoint(testObject.transform.position) / screenSize;
            yield return wait;
        }

        testObject.ChangeState(to);

        pauseRoutine = null;
    }

    private IEnumerator ExitPauseAnimation(RecallState from, RecallState to)
    {
        testObject.ChangeState(from);
        var wait = new WaitForEndOfFrame();

        recallEffect.wipeSize.value = 0.0f;

        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime)
        {
            yield return wait;
        }

        testObject.ChangeState(to);

        pauseRoutine = null;
    }
}
