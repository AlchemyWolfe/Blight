using DG.Tweening;
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
    Pause,
    Skins,
    Weapons
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

    public GameOptionsSO Options;
    public AudioListener UIAudioListener;
    public Camera MainCamera;

    private FullScreenMenuController ActiveMenu;
    private BackgroundController ActiveBackground;

    private string CurrentSceneName;
    private FullscreenMenuType PendingOpenMenu;
    private bool Paused;
    private FullScreenMenuController PauseMenu;

    void Awake()
    {
        foreach (var menu in Menus)
        {
            menu.MenuChangeRequested += OnMenuChangeRequested;
            menu.SceneChangeRequested += OnSceneChangeRequested;
            //menu.LoadGameRequested += OnLoadGameRequested;
            menu.gameObject.SetActive(false);
            if (menu.Type == FullscreenMenuType.Pause)
            {
                PauseMenu = menu;
            }
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
        loadingScreen.OnFinishedLoading += OnFinishedLoadingGameReceived;
        loadingScreen.StartLoading();
        return;
    }
    */

    private void OnFinishedLoadingGameReceived(LoadingScreenController controller)
    {
        controller.OnFinishedLoading -= OnFinishedLoadingGameReceived;
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
        loadingScreen.OnFinishedLoading += OnFinishedLoadingSceneReceived;
        loadingScreen.StartLoading();
        return;
    }

    private void OnFinishedLoadingSceneReceived(LoadingScreenController controller)
    {
        controller.OnFinishedLoading -= OnFinishedLoadingSceneReceived;
        //controller.CloseMenu();
        SwitchMenu(FullscreenMenuType.Game);
    }

    private void OnMenuChangeRequested(FullscreenMenuType type)
    {
        SwitchMenu(type);
    }

    private void SwitchBackground(BackgroundController newBackground)
    {
        if (ActiveBackground == newBackground)
        {
            return;
        }

        if (ActiveBackground != null)
        {
            ActiveBackground.OnFinishedHiding += OnFinishedHidingBackgroundReceived;
            ActiveBackground.HideBackground();
        }

        ActiveBackground = newBackground;
        if (ActiveBackground != null)
        {
            ActiveBackground.OnFinishedShowing += OnFinishedShowingBackgroundReceived;
            ActiveBackground.gameObject.SetActive(true);
            ActiveBackground.ShowBackground();
        }
    }

    private void OnFinishedHidingBackgroundReceived(BackgroundController background)
    {
        background.OnFinishedHiding -= OnFinishedHidingBackgroundReceived;
        background.gameObject.SetActive(false);
        DoPendingOpenActions();
    }

    private void OnFinishedShowingBackgroundReceived(BackgroundController background)
    {
        background.OnFinishedShowing -= OnFinishedShowingBackgroundReceived;
        DoPendingOpenActions();
    }

    private void SwitchMenu(FullscreenMenuType type)
    {
        if (type == FullscreenMenuType.Pause)
        {
            Time.timeScale = 0f;
            DOTween.PauseAll();
            Paused = true;
            ActiveMenu.gameObject.SetActive(false);
            ActiveMenu.EnableControls(false);
            PauseMenu.gameObject.SetActive(true);
            PauseMenu.OpenMenu();
            PauseMenu.EnableControls(true);
            PauseMenu.background.gameObject.SetActive(true);
            PauseMenu.background.ShowBackground();
            return;
        }
        if (Paused)
        {
            Time.timeScale = 1f;
            DOTween.PlayAll();
            Paused = false;
            if (ActiveMenu.Type == type)
            {
                ActiveMenu.gameObject.SetActive(true);
                ActiveMenu.EnableControls(true);
                // Just hide the pasue menu.
                if (ActiveBackground != PauseMenu.background)
                {
                    PauseMenu.background.OnFinishedHiding += OnFinishedHidingBackgroundReceived;
                    PauseMenu.background.HideBackground();
                }
                PendingOpenMenu = FullscreenMenuType.None;
                PauseMenu.OnFinishedClosing += OnFinishedClosingMenuReceived;
                PauseMenu.EnableControls(false);
                PauseMenu.CloseMenu();
                return;
            }
        }
        if (ActiveMenu != null)
        {
            ActiveMenu.OnFinishedClosing += OnFinishedClosingMenuReceived;
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

    private void OnFinishedClosingMenuReceived(FullScreenMenuController menu)
    {
        menu.OnFinishedClosing -= OnFinishedClosingMenuReceived;
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
