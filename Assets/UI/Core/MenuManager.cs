using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum FullscreenMenuType
{
    None,
    MainMenu,
    Credits,
    LoadingScreen,
    Options,
    Game,
    Pause
}

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("These are full screen backgrounds that could be used by any menu.")]
    private List<BackgroundController> _backgrounds;
    private List<BackgroundController> Backgrounds => _backgrounds;

    [SerializeField]
    [Tooltip("These are mutually exclusive menus.  Opening one will close the others.")]
    private List<FullScreenMenuController> _menus;
    private List<FullScreenMenuController> Menus => _menus;

    public AudioListener UIAudioListener;
    public Camera MainCamera;

    private FullScreenMenuController ActiveMenu;
    private BackgroundController ActiveBackground;

    private string CurrentSceneName;
    private FullscreenMenuType PendingOpenMenu;

    void Awake()
    {
        foreach (var menu in Menus)
        {
            menu.MenuChangeRequested += OnMenuChangeRequested;
            menu.SceneChangeRequested += OnSceneChangeRequested;
            //menu.LoadGameRequested += OnLoadGameRequested;
            menu.gameObject.SetActive(false);
        }
        Backgrounds.ForEach(background => background.gameObject.SetActive(false));
        if (ActiveBackground == null || !ActiveBackground.WaitForOpen)
        {
            SwitchMenu(FullscreenMenuType.MainMenu);
        }
        else
        {
            PendingOpenMenu = FullscreenMenuType.MainMenu;
        }
    }

    /*
    private void OnLoadGameRequested(SaveGameEntry entry)
    {
        if (entry == null)
        {
            // Do we want to start a new game here?
            return;
        }

        SwitchMenu(FullscreenMenuType.LoadingScreen);
        var loadingScreen = ActiveMenu as LoadingScreenController;
        if (loadingScreen == null)
        {
            Debug.LogError("No Loading Screen.  Loading scene anyway.");
            // Do not make this pretty.
            SceneManager.LoadScene(entry.SceneName);
            return;
        }

        if (!string.IsNullOrEmpty(entry.SaveName))
        {
            loadingScreen.AddSavegameLoad(entry.SaveName);
        }

        if (!string.IsNullOrEmpty(CurrentSceneName))
        {
            loadingScreen.AddSceneUnload(CurrentSceneName);
        }
        loadingScreen.AddSceneLoad(entry.SceneName);

        CurrentSceneName = entry.SceneName;
        loadingScreen.OnFinishedLoading += OnFinishedLoadingGameRecieved;
        loadingScreen.StartLoading();
        return;
    }
    */

    private void OnFinishedLoadingGameRecieved(LoadingScreenController controller)
    {
        controller.OnFinishedLoading -= OnFinishedLoadingGameRecieved;
        SwitchMenu(FullscreenMenuType.Game);
        if (ActiveBackground != null)
        {
            ActiveBackground.HideBackground();
        }
    }

    private void OnSceneChangeRequested(string sceneName)
    {
        SwitchMenu(FullscreenMenuType.LoadingScreen);
        var loadingScreen = ActiveMenu as LoadingScreenController;
        if (!string.IsNullOrEmpty(CurrentSceneName))
        {
            loadingScreen.AddSceneUnload(CurrentSceneName);
        }
        loadingScreen.AddSceneLoad(sceneName);
        var scene = SceneManager.GetSceneByName(sceneName);
        CurrentSceneName = sceneName;
        loadingScreen.OnFinishedLoading += OnFinishedLoadingSceneRecieved;
        loadingScreen.StartLoading();
        return;
    }

    private void OnFinishedLoadingSceneRecieved(LoadingScreenController controller)
    {
        controller.OnFinishedLoading -= OnFinishedLoadingSceneRecieved;
        //controller.CloseMenu();
        SwitchMenu(FullscreenMenuType.Game);
    }

    private void OnMenuChangeRequested(FullscreenMenuType type)
    {
        SwitchMenu(type);
    }

    private void SwitchBackground(BackgroundController newBackground)
    {
        var oldName = ActiveBackground ? ActiveBackground.name : "null";
        var newName = newBackground ? newBackground.name : "null";
        Debug.Log("SwitchBackground from " + oldName + " to " + newName);
        if (ActiveBackground == newBackground)
        {
            return;
        }

        if (ActiveBackground != null)
        {
            ActiveBackground.OnFinishedHiding += OnFinishedHidingBackgroundRecieved;
            Debug.Log("Hiding " + ActiveBackground.name + " start");
            ActiveBackground.HideBackground();
        }

        ActiveBackground = newBackground;
        if (ActiveBackground != null)
        {
            ActiveBackground.OnFinishedShowing += OnFinishedShowingBackgroundRecieved;
            ActiveBackground.gameObject.SetActive(true);
            Debug.Log("Showing " + ActiveBackground.name + " start");
            ActiveBackground.ShowBackground();
        }
    }

    private void OnFinishedHidingBackgroundRecieved(BackgroundController background)
    {
        Debug.Log("Hiding " + background.name + " end");
        background.OnFinishedHiding -= OnFinishedHidingBackgroundRecieved;
        background.gameObject.SetActive(false);
        DoPendingOpenActions();
    }

    private void OnFinishedShowingBackgroundRecieved(BackgroundController background)
    {
        Debug.Log("Showing " + background.name + " end");
        background.OnFinishedShowing -= OnFinishedShowingBackgroundRecieved;
        DoPendingOpenActions();
    }

    private void SwitchMenu(FullscreenMenuType type)
    {
        if (ActiveMenu != null)
        {
            ActiveMenu.OnFinishedClosing += OnFinishedClosingMenuRecieved;
            ActiveMenu.EnableControls(false);
            ActiveMenu.CloseMenu();
        }

        ActiveMenu = Menus.Find(menu => menu.Type == type);
        if (ActiveMenu != null)
        {
            ActiveMenu.gameObject.SetActive(true);
            ActiveMenu.OpenMenu();
            ActiveMenu.EnableControls(true);
            SwitchBackground(ActiveMenu.background);
        }
        else
        {
            SwitchBackground(null);
        }
        MainCamera.enabled = type != FullscreenMenuType.Game;
        UIAudioListener.enabled = type != FullscreenMenuType.Game;
    }

    private void OnFinishedClosingMenuRecieved(FullScreenMenuController menu)
    {
        menu.OnFinishedClosing -= OnFinishedClosingMenuRecieved;
        menu.gameObject.SetActive(false);
        DoPendingOpenActions();
    }

    private void DoPendingOpenActions()
    {
        if (PendingOpenMenu != FullscreenMenuType.None)
        {
            var menuType = PendingOpenMenu;
            PendingOpenMenu = FullscreenMenuType.None;
            SwitchMenu(menuType);
        }
    }
}
