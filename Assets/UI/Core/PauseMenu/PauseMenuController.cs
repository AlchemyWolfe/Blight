using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : FullScreenMenuController
{
    public GameOptionsSO Options;
    [SerializeField]
    private Button _closeButton;
    public Button CloseButton => _closeButton;

    [SerializeField]
    private Button _exitButton;
    public Button ExitButton => _exitButton;

    [SerializeField]
    private Button _volumeButton;
    public Button VolumeButton => _volumeButton;

    [SerializeField]
    private Image _muteIcon;
    public Image MuteIcon => _muteIcon;

    [SerializeField]
    private Slider _volumeSlider;
    public Slider VolumeSlider => _volumeSlider;

    public override FullscreenMenuType Type { get => FullscreenMenuType.Pause; }

    public override void EnableControls(bool enabled)
    {
        CloseButton.enabled = enabled;
        ExitButton.enabled = enabled;
    }

    private void OnCloseButtonClicked()
    {
        PauseToggleRequested?.Invoke();
    }

    private void OnExitButtonClicked()
    {
        PauseToggleRequested?.Invoke();
        SceneChangeRequested?.Invoke(null);
    }

    private void OnVolumeButtonClicked()
    {
        Options.Mute = !Options.Mute;
        MuteIcon.enabled = Options.Mute;
        AudioListener.volume = Options.Mute ? 0f : Options.Volume;
    }

    private void OnVolumeValueChanged(float value)
    {
        Options.Volume = value;
        AudioListener.volume = Options.Volume;
    }

    public override void CloseMenu(float fade = 0)
    {
        CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        ExitButton.onClick.RemoveListener(OnExitButtonClicked);
        VolumeButton.onClick.RemoveListener(OnVolumeButtonClicked);
        VolumeSlider.onValueChanged.RemoveListener(OnVolumeValueChanged);
        base.CloseMenu(fade);
    }

    public override void OpenMenu(float fade = 0)
    {
        MuteIcon.enabled = Options.Mute;
        VolumeSlider.value = Options.Volume;
        CloseButton.onClick.AddListener(OnCloseButtonClicked);
        ExitButton.onClick.AddListener(OnExitButtonClicked);
        VolumeButton.onClick.AddListener(OnVolumeButtonClicked);
        VolumeSlider.onValueChanged.AddListener(OnVolumeValueChanged);
        base.OpenMenu(fade);
    }
}
