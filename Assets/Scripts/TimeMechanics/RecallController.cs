using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallController : MonoBehaviour
{
    [SerializeField] private RecallObject testObject;

    private bool isRecall = false;
    private Coroutine pauseRoutine = null;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && pauseRoutine == null)
        {
            isRecall = !isRecall;

            if (isRecall)
            {
                pauseRoutine = StartCoroutine(PauseAnimation(RecallState.Pause, RecallState.Recall));
                testObject.RecallBufferExpired += ExpireRecallMemory;
            }
            else
            {
                pauseRoutine = StartCoroutine(PauseAnimation(RecallState.Pause, RecallState.Advance));
                testObject.RecallBufferExpired -= ExpireRecallMemory;
            }
        }
    }

    public void ExpireRecallMemory(object sender, EventArgs e)
    {
        isRecall = false;
        pauseRoutine = StartCoroutine(PauseAnimation(RecallState.Pause, RecallState.Advance));
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
