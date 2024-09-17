using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : FullScreenMenuController
{
    public GameOptionsSO Options;
    public GameSceneToolsSO Tools;

    [SerializeField]
    private TMP_Text _scoreText;
    public TMP_Text ScoreText => _scoreText;
    [SerializeField]
    private Image _scoreIcon;
    public Image ScoreIcon => _scoreIcon;
    [SerializeField]
    private TMP_Text _gemsText;
    public TMP_Text GemsText => _gemsText;
    [SerializeField]
    private Image _gemsIcon;
    public Image GemsIcon => _gemsIcon;
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
    private Button _quitButton;
    public Button QuitButton => _quitButton;
    [SerializeField]
    private string _gameScene;
    public string GameScene => _gameScene;
    [SerializeField]
    private PlayerDataSO _playerData;
    public PlayerDataSO PlayerData
    {
        get
        {
            return _playerData;
        }
        set
        {
            _playerData = value;
            RefreshPlayerData();
        }
    }

    private Tween ScoreTween = null;
    private Tween GemsTween = null;

    public override FullscreenMenuType Type { get => FullscreenMenuType.MainMenu; }

    void Awake()
    {
        AudioListener.volume = Options.Mute ? 0f : Options.Volume;
        ScoreIcon.enabled = false;
        ScoreText.text = string.Empty;
        GemsIcon.enabled = false;
        GemsText.text = string.Empty;
    }

    void Start()
    {
        NewGameButton.onClick.AddListener(OnNewGameButtonClicked);
        SkinsButton.onClick.AddListener(OnSkinsButtonClicked);
        WeaponsButton.onClick.AddListener(OnWeaponsButtonClicked);
        OptionsButton.onClick.AddListener(OnOptionsButtonClicked);
        VolumeButton.onClick.AddListener(OnVolumeButtonClicked);
        CreditsButton.onClick.AddListener(OnCreditsButtonClicked);
        QuitButton.onClick.AddListener(OnQuitButtonClicked);
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
        try
        {
            Options.Load();
            PlayerData.Load();
        }
        catch
        {
            Options.Save();
            PlayerData.Save();
        }
        MuteIcon.enabled = Options.Mute;
        VolumeSlider.value = Options.Volume;
        if (PlayerData.HighScore > 0)
        {
            var highScore = (int)PlayerData.HighScore;
            ScoreText.text = highScore.ToString();
            ScoreIcon.enabled = true;
        }
        if (PlayerData.TotalGems > 0)
        {
            var availableGems = PlayerData.TotalGems - Options.CurrentWeaponCost;
            GemsText.text = availableGems.ToString();
            GemsIcon.enabled = true;
        }
    }

    private void StartGame()
    {
        Tools.OnGameStart?.Invoke();
        MenuChangeRequested?.Invoke(FullscreenMenuType.Game);
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
        AudioListener.volume = Options.Mute ? 0f : Options.Volume;
    }

    private void OnVolumeValueChanged(float value)
    {
        Options.Volume = value;
        AudioListener.volume = Options.Volume;
    }

    private void OnCreditsButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.Credits);
    }

    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    private void RefreshPlayerData()
    {
        if (PlayerData == null)
        {
            return;
        }
        PlayerData.OnTotalGemsChanged += OnTotalGemsChangedReceived;
    }

    private void OnTotalGemsChangedReceived()
    {
        GemsText.text = PlayerData.TotalGems.ToString();
    }

    public override void CloseMenu(float fade = 0)
    {
        ScoreTween?.Kill();
        GemsTween?.Kill();
        base.CloseMenu(fade);
    }

    public override void OpenMenu(float fade = 0)
    {
        MuteIcon.enabled = Options.Mute;
        VolumeSlider.value = Options.Volume;
        base.OpenMenu(fade);
        UpdateScore();
        UpdateGems();
    }

    public void UpdateScore()
    {
        if (PlayerData == null || PlayerData.HighScore == 0)
        {
            ScoreIcon.enabled = false;
            ScoreText.text = string.Empty;
            return;
        }
        if (PlayerData.PreviousHighScore == 0)
        {
            PlayerData.PreviousHighScore = PlayerData.HighScore;
        }
        ScoreIcon.enabled = true;
        ScoreText.text = PlayerData.PreviousHighScore.ToString();
        if (PlayerData.PreviousHighScore < PlayerData.HighScore)
        {
            var duration = 3.0f;
            var updateRate = 0.02f;
            int updateCount = (int)(duration / updateRate);
            var scoreDifference = PlayerData.HighScore - PlayerData.PreviousHighScore;
            var scoreStep = Mathf.Max(100f, scoreDifference / updateCount);
            updateCount = (int)(scoreDifference / scoreStep);
            var scoreDisplay = PlayerData.PreviousHighScore;
            ScoreTween?.Kill();
            ScoreTween = DOVirtual.DelayedCall(
                    updateRate,
                    () =>
                    {
                        scoreDisplay = Mathf.Min(scoreDisplay + scoreStep, PlayerData.HighScore);
                        int scoreIntDisplay = (int)scoreDisplay;
                        ScoreText.text = scoreIntDisplay.ToString();
                    }
                )
                .SetLoops(updateCount)
                .OnKill(() => ScoreTween = null);
        }
    }

    public void UpdateGems()
    {
        if (PlayerData == null || PlayerData.TotalGems == 0)
        {
            GemsIcon.enabled = false;
            GemsText.text = string.Empty;
            return;
        }
        var availableGems = PlayerData.TotalGems - Options.CurrentWeaponCost;
        if (PlayerData.PreviousGems == 0)
        {
            PlayerData.PreviousGems = availableGems;
        }
        GemsIcon.enabled = true;
        GemsText.text = PlayerData.PreviousGems.ToString();
        if (PlayerData.PreviousGems != availableGems)
        {
            var duration = 3.0f;
            var updateRate = 0.02f;
            int updateCount = (int)(duration / updateRate);
            var gemsDifference = availableGems - PlayerData.PreviousGems;
            var gemsStep = gemsDifference > 0 ? Mathf.Max(0.5f, gemsDifference / updateCount) : Mathf.Min(-0.5f, gemsDifference / updateCount);
            updateCount = (int)(gemsDifference / gemsStep);
            var gemsDisplay = PlayerData.PreviousGems;
            GemsTween?.Kill();
            GemsTween = DOVirtual.DelayedCall(
                    updateRate,
                    () =>
                    {
                        gemsDisplay = gemsDifference > 0 ? Mathf.Min(gemsDisplay + gemsStep, availableGems) : Mathf.Max(gemsDisplay + gemsStep, availableGems);
                        int gemsIntDisplay = (int)gemsDisplay;
                        GemsText.text = gemsIntDisplay.ToString();
                    }
                )
                .SetLoops(updateCount)
                .OnKill(() => GemsTween = null);
        }
    }
}
