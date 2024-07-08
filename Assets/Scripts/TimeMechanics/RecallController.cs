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

    private bool isRecall = false;
    private Coroutine pauseRoutine = null;

    private void Start()
    {
        if(recallVolume != null)
        {
            recallVolume.profile.TryGet(out recallEffect);
        }

        recallEffect.active = false;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(pauseRoutine == null)
            {
                isRecall = !isRecall;

                if (isRecall)
                {
                    recallEffect.active = true;
                    pauseRoutine = StartCoroutine(PauseAnimation(RecallState.EnterPause, RecallState.Recall));
                    testObject.RecallBufferExpired += ExpireRecallMemory;
                }
                else
                {
                    recallEffect.active = false;
                    pauseRoutine = StartCoroutine(PauseAnimation(RecallState.ExitPause, RecallState.Advance));
                    testObject.RecallBufferExpired -= ExpireRecallMemory;
                }
            }
        }
    }

    public void ExpireRecallMemory(object sender, EventArgs e)
    {
        recallEffect.active = false;
        isRecall = false;
        pauseRoutine = StartCoroutine(PauseAnimation(RecallState.ExitPause, RecallState.Advance));
        testObject.RecallBufferExpired -= ExpireRecallMemory;
    }

    private IEnumerator PauseAnimation(RecallState from, RecallState to)
    {
        testObject.ChangeState(from);
        yield return new WaitForSeconds(1.0f);
        testObject.ChangeState(to);

        pauseRoutine = null;
    }
}
