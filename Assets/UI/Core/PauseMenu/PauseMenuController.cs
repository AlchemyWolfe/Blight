using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : FullScreenMenuController
{
    [SerializeField]
    private Button _closeButton;
    public Button CloseButton => _closeButton;

    public override FullscreenMenuType Type { get => FullscreenMenuType.Pause; }

    public override void EnableControls(bool enabled)
    {
        CloseButton.enabled = enabled;
    }

    private void OnCloseButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.Game);
    }

    public override void CloseMenu(float fade = 0)
    {
        CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        base.CloseMenu(fade);
    }

    public override void OpenMenu(float fade = 0)
    {
        CloseButton.onClick.AddListener(OnCloseButtonClicked);
        base.OpenMenu(fade);
    }
}
