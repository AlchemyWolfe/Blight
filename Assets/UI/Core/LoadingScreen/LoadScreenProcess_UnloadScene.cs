using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScreenProcess_UnloadScene : LoadingScreenProcess
{
    [SerializeField]
    private string _sceneName;
    public string SceneName { get => _sceneName; set => _sceneName = value; }

    public override void GesstimateDuration()
    {
        ExpectedDuration = 0.01f;
    }

    public override void StartLoading()
    {
        StartCoroutine(UnloadSceneAsync());
    }

    public IEnumerator UnloadSceneAsync()
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(SceneName);

        // Disable scene activation to prevent auto-loading
        asyncUnload.allowSceneActivation = false;

        // Wait until the asynchronous operation is complete
        while (!asyncUnload.isDone)
        {
            // Update the progress bar based on the loading progress
            InternalProgress = Mathf.Clamp01(asyncUnload.progress / 0.9f);

            // Check if the loading is almost complete (0.9 is used as a threshold)
            if (asyncUnload.progress >= 0.9f)
            {
                InternalProgress = 1f; // Ensure the progress bar is full
                asyncUnload.allowSceneActivation = true; // Allow the scene to activate
            }
            OnProgress?.Invoke(this, InternalProgress);

            yield return null;
        }
        OnProgress?.Invoke(this, 1f);
        OnComplete?.Invoke(this);
    }
}