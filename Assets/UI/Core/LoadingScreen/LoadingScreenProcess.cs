using System;
using UnityEngine;

// The idea for this is to ecapsulate each part of a load, and allow it to show it's own pogress on top of the total load process
public class LoadingScreenProcess : MonoBehaviour
{
    [SerializeField]
    private string _loadingBarDisplayKey;
    public string LoadingBarDisplayKey { get => _loadingBarDisplayKey; set => _loadingBarDisplayKey = value; }

    public float ExpectedDuration { get; set; }
    public float StartPercentage { get; set; }
    public float EndPercentage { get; set; }
    public Action<LoadingScreenProcess, float> OnProgress { get; set; }
    public Action<LoadingScreenProcess> OnComplete { get; set; }

    // Range from 0f to 1f, this is the progress for this process.
    public float InternalProgress { get; set; }

    // Range from StartPercentage to EndPercentage, this is the progress within the total LoadOrder.
    public float TotalProgress { get => Mathf.Lerp(StartPercentage, EndPercentage, InternalProgress); }

    // Set ExpectedDuration to how long you think this loading process will take.
    public virtual void GesstimateDuration()
    {
        ExpectedDuration = 1f;
    }

    // All loading for this segment.
    public virtual void StartLoading()
    {
        InternalProgress = 1f;
        OnProgress?.Invoke(this, 1f);
        OnComplete?.Invoke(this);
    }
}