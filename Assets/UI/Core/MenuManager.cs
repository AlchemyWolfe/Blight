using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

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
    Weapons,
    GameOver,
    Upgrade
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

    public GameSceneToolsSO Tools;
    public GameOptionsSO Options;

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
            menu.PauseToggleRequested += OnPauseToggleRequested;
            menu.MenuChangeRequested += OnMenuChangeRequested;
            //menu.SceneChangeRequested += OnSceneChangeRequested;
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
        Tools.OnGameOver += OnGameOverReceived;
    }

    public void OnGameOverReceived()
    {
        SwitchMenu(FullscreenMenuType.GameOver);
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

    private void OnPauseToggleRequested()
    {
        if (Paused)
        {
            Time.timeScale = 1f;
            DOTween.PlayAll();
            Paused = false;
            ActiveMenu.gameObject.SetActive(true);
            ActiveMenu.EnableControls(true);
            // Just hide the pasue menu.
            if (PauseMenu.background != null)
            {
                PauseMenu.background.OnFinishedHiding += OnFinishedHidingBackgroundReceived;
                PauseMenu.background.HideBackground();
            }
            PendingOpenMenu = FullscreenMenuType.None;
            PauseMenu.OnFinishedClosing += OnFinishedClosingMenuReceived;
            PauseMenu.EnableControls(false);
            PauseMenu.CloseMenu();
            ActiveMenu.gameObject.SetActive(true);
            ActiveMenu.EnableControls(true);
        }
        else
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
    }

    public void SwitchMenu(FullscreenMenuType type)
    {
        if (type == FullscreenMenuType.Pause)
        {
            Debug.LogWarning("Do not pause in this way.");
            OnPauseToggleRequested();
            return;
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
