using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : FullScreenMenuController
{
    [SerializeField]
    private Button _closeButton;
    public Button CloseButton => _closeButton;

    [SerializeField]
    private Button _exitButton;
    public Button ExitButton => _exitButton;

    public override FullscreenMenuType Type { get => FullscreenMenuType.Pause; }

    public override void EnableControls(bool enabled)
    {
        CloseButton.enabled = enabled;
        ExitButton.enabled = enabled;
    }

    private void OnCloseButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.Game);
    }

    private void OnExitButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.MainMenu);
        SceneChangeRequested?.Invoke(null);
    }

    public override void CloseMenu(float fade = 0)
    {
        CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        ExitButton.onClick.RemoveListener(OnExitButtonClicked);
        base.CloseMenu(fade);
    }

    public override void OpenMenu(float fade = 0)
    {
        CloseButton.onClick.AddListener(OnCloseButtonClicked);
        ExitButton.onClick.AddListener(OnExitButtonClicked);
        base.OpenMenu(fade);
    }
}
