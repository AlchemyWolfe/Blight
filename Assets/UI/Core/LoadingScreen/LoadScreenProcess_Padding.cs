using System;
using System.Collections;
using UnityEngine;

public class LoadScreenProcess_Padding : LoadingScreenProcess
{
    [SerializeField]
    private float _paddingTime;
    public float PaddingTime { get => _paddingTime; set => _paddingTime = value; }

    public override void GesstimateDuration()
    {
        ExpectedDuration = PaddingTime;
    }

    public override void StartLoading()
    {
        // Simulate loading process completion asynchronously (using coroutines in Unity)
        StartCoroutine(SimulateLoading());
    }

    // Simulates the loading process using a coroutine
    private IEnumerator SimulateLoading()
    {
        var elapsedTime = 0f;
        while (elapsedTime < ExpectedDuration)
        {
            elapsedTime += Time.deltaTime;
            InternalProgress = Math.Min(elapsedTime / ExpectedDuration, 1f);
            OnProgress?.Invoke(this, TotalProgress);
            yield return null;
        }

        OnProgress?.Invoke(this, EndPercentage);
        OnComplete?.Invoke(this);
    }
}
