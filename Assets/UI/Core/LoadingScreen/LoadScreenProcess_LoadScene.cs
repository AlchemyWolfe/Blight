using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScreenProcess_LoadScene : LoadingScreenProcess
{
    [SerializeField]
    private string _sceneName;
    public string SceneName { get => _sceneName; set => _sceneName = value; }

    public override void GesstimateDuration()
    {
        ExpectedDuration = 0.010f;
    }

    public override void StartLoading()
    {
        StartCoroutine(LoadSceneAsync());
    }

    public IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);

        // Disable scene activation to prevent auto-loading
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous operation is complete
        while (!asyncLoad.isDone)
        {
            // Update the progress bar based on the loading progress
            InternalProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Check if the loading is almost complete (0.9 is used as a threshold)
            if (asyncLoad.progress >= 0.9f)
            {
                InternalProgress = 1f; // Ensure the progress bar is full
                asyncLoad.allowSceneActivation = true; // Allow the scene to activate
            }
            OnProgress?.Invoke(this, InternalProgress);

            yield return null;
        }
        OnProgress?.Invoke(this, 1f);
        OnComplete?.Invoke(this);
    }
}
