using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenController : FullScreenMenuController
{
    [SerializeField]
    private Slider _loadingSlider;
    public Slider LoadingSlider => _loadingSlider;

    [SerializeField]
    private RandomTextController _loadingSliderText;
    public RandomTextController LoadingSliderText => _loadingSliderText;

    [SerializeField]
    private bool _showRandomText;
    public bool ShowRandomText => _showRandomText;

    public override FullscreenMenuType Type { get => FullscreenMenuType.LoadingScreen; }
    [SerializeField]
    public List<LoadingScreenProcess> LoadOrder = new List<LoadingScreenProcess>();
    private int CurrentLoadingIndex { get; set; }
    private LoadingScreenProcess CurrentLoadingProcess { get; set; }
    private bool ShowingCustomSliderText { get; set; }
    public Action<LoadingScreenController> OnFinishedLoading { get; set; }

    private void Awake()
    {
        LoadingSliderText?.Pause();
    }

    public override void EnableControls(bool enabled)
    {
    }

    public void CalculatePercentagesBasedOnExpectedDuration()
    {
        var totalDuration = 0f;

        foreach (var loadingProcess in LoadOrder)
        {
            loadingProcess.GesstimateDuration();
            if (loadingProcess.ExpectedDuration <= 0)
            {
                loadingProcess.ExpectedDuration = 1f;
            }
        }

        foreach (var loadingProcess in LoadOrder)
        {
            totalDuration += loadingProcess.ExpectedDuration;
        }

        var accumulatedPercentage = 0f;

        for (var i = 0; i < LoadOrder.Count; i++)
        {
            var loadingProcess = LoadOrder[i];
            var percentage = loadingProcess.ExpectedDuration / totalDuration;
            loadingProcess.StartPercentage = accumulatedPercentage;
            loadingProcess.EndPercentage = accumulatedPercentage + percentage;
            accumulatedPercentage += percentage;
        }
    }

    public void StartLoading()
    {
        if (LoadOrder.Count == 0)
        {
            CloseMenu();
            return;
        }

        CalculatePercentagesBasedOnExpectedDuration();

        // Start the loading process for the first item
        CurrentLoadingIndex = 0;
        StartLoadingProcess(LoadOrder[CurrentLoadingIndex]);
    }

    private void StartLoadingProcess(LoadingScreenProcess loadingProcess)
    {
        CurrentLoadingProcess = loadingProcess;
        loadingProcess.OnProgress += OnProgressReceived;
        loadingProcess.OnComplete += OnCompleteReceived;
        loadingProcess.StartLoading();
        if (loadingProcess.InternalProgress >= 1f)
        {
            // Some things might be instant, and we don't want to stomp the next thing.
            return;
        }
        if (!ShowRandomText && !string.IsNullOrEmpty(loadingProcess.LoadingBarDisplayKey) && LoadingSliderText != null)
        {
            ShowingCustomSliderText = true;
            LoadingSliderText.ShowNonRandomText(loadingProcess.LoadingBarDisplayKey);
        }
        else
        {
            ShowingCustomSliderText = false;
        }
        LoadingSliderText?.Pause(ShowingCustomSliderText);
    }

    // Update the loading progress in the UI slider
    private void OnProgressReceived(LoadingScreenProcess process, float progress)
    {
        if (LoadingSlider != null)
        {
            LoadingSlider.value = progress;
        }
        if (ShowingCustomSliderText)
        {
            LoadingSliderText.UpdateNonRandomTextProgress(CurrentLoadingProcess.InternalProgress);
        }
    }

    // Callback when the loading is complete for an ILoadingScreenProcess
    private void OnCompleteReceived(LoadingScreenProcess finishedProcess)
    {
        finishedProcess.OnProgress -= OnProgressReceived;
        finishedProcess.OnComplete -= OnCompleteReceived;

        CurrentLoadingIndex++;

        if (CurrentLoadingIndex < LoadOrder.Count)
        {
            StartLoadingProcess(LoadOrder[CurrentLoadingIndex]);
        }
        else
        {
            OnFinishedLoading?.Invoke(this);
        }
    }

    public void ClearLoadOrder()
    {
        foreach (var process in LoadOrder)
        {
            Destroy(process);
        }
        LoadOrder.Clear();
    }

    public override void CloseMenu(float fade = 0f)
    {
        ClearLoadOrder();
        LoadingSliderText?.Pause();
        gameObject.SetActive(false);
    }

    public void AddLoadPadding(string displayKey, float duration)
    {
        var process = gameObject.AddComponent<LoadScreenProcess_Padding>();
        process.PaddingTime = duration;
        process.LoadingBarDisplayKey = displayKey;
        LoadOrder.Add(process);
    }

    public void AddSavegameLoad(string saveName)
    {
    }

    public void AddSceneUnload(string sceneName)
    {
        var process = gameObject.AddComponent<LoadScreenProcess_UnloadScene>();
        process.LoadingBarDisplayKey = "Unloading " + sceneName;
        process.SceneName = sceneName;
        LoadOrder.Add(process);
    }

    public void AddSceneLoad(string sceneName)
    {
        var process = gameObject.AddComponent<LoadScreenProcess_LoadScene>();
        process.LoadingBarDisplayKey = "Loading " + sceneName;
        process.SceneName = sceneName;
        LoadOrder.Add(process);
    }
}
