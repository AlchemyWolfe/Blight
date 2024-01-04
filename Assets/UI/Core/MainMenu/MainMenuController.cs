using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : FullScreenMenuController
{
    public GameOptionsSO Options;
    [SerializeField]
    private Button _newGameButton;
    public Button NewGameButton => _newGameButton;
    [SerializeField]
    private Button _skinsButton;
    public Button SkinsButton => _skinsButton;
    [SerializeField]
    private Button _weaponsButton;
    public Button WeaponsButton => _weaponsButton;
    [SerializeField]
    private Button _optionsButton;
    public Button OptionsButton => _optionsButton;
    [SerializeField]
    private Button _volumeButton;
    public Button VolumeButton => _volumeButton;
    [SerializeField]
    private Image _muteIcon;
    public Image MuteIcon => _muteIcon;
    [SerializeField]
    private Slider _volumeSlider;
    public Slider VolumeSlider => _volumeSlider;
    [SerializeField]
    private Button _creditsButton;
    public Button CreditsButton => _creditsButton;
    [SerializeField]
    private string _gameScene;
    public string GameScene => _gameScene;

    public override FullscreenMenuType Type { get => FullscreenMenuType.MainMenu; }
    //private SavegameEntry ContinueSave { get; set; }

    void Start()
    {
        NewGameButton.onClick.AddListener(OnNewGameButtonClicked);
        SkinsButton.onClick.AddListener(OnSkinsButtonClicked);
        WeaponsButton.onClick.AddListener(OnWeaponsButtonClicked);
        OptionsButton.onClick.AddListener(OnOptionsButtonClicked);
        VolumeButton.onClick.AddListener(OnVolumeButtonClicked);
        CreditsButton.onClick.AddListener(OnCreditsButtonClicked);
        VolumeSlider.onValueChanged.AddListener(OnVolumeValueChanged);

        FindExistingGame();
    }

    public override void EnableControls(bool enabled)
    {
        NewGameButton.enabled = enabled;
        SkinsButton.enabled = enabled;
        WeaponsButton.enabled = enabled;
        OptionsButton.enabled = enabled;
        VolumeButton.enabled = enabled;
        CreditsButton.enabled = enabled;
    }

    private void FindExistingGame()
    {
        MuteIcon.enabled = Options.Mute;
        VolumeSlider.value = Options.Volume;
    }

    private void StartGame()
    {
        SceneChangeRequested?.Invoke(GameScene);
    }

    private void OnNewGameButtonClicked()
    {
        StartGame();
    }

    private void OnSkinsButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.Skins);
    }

    private void OnWeaponsButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.Weapons);
    }

    private void OnOptionsButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.Options);
    }

    private void OnVolumeButtonClicked()
    {
        Options.Mute = !Options.Mute;
        MuteIcon.enabled = Options.Mute;
    }

    private void OnVolumeValueChanged(float value)
    {
        Options.Volume = value;
    }

    private void OnCreditsButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.Credits);
    }
}
