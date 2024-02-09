using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuController : FullScreenMenuController
{
    public GameSceneToolsSO Tools;

    [SerializeField]
    private PlayerDataSO _playerData;
    public PlayerDataSO PlayerData => _playerData;

    [SerializeField]
    private TMP_Text _score;
    public TMP_Text Score => _score;

    [SerializeField]
    private TMP_Text _gems;
    public TMP_Text Gems => _gems;

    [SerializeField]
    private Image _shieldIcon;
    public Image ShieldIcon => _shieldIcon;

    [SerializeField]
    private TMP_Text _shieldText;
    public TMP_Text ShieldText => _shieldText;

    [SerializeField]
    private TMP_Text _wave;
    public TMP_Text Wave => _wave;

    [SerializeField]
    private Button _pauseButton;
    public Button PauseButton => _pauseButton;

    [SerializeField]
    private Button _barkButton;
    public Button BarkButton => _barkButton;

    public override FullscreenMenuType Type { get => FullscreenMenuType.Game; }

    void Start()
    {
        PauseButton.onClick.AddListener(OnPauseButtonClicked);
        BarkButton.onClick.AddListener(OnBarkButtonClicked);
    }

    private void OnPauseButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.Pause);
    }

    private void OnBarkButtonClicked()
    {
        if (Tools.Player != null)
        {
            Tools.Player.Bark();
        }
    }

    public void OnGameGemsChangedReceived()
    {
        Gems.text = PlayerData.GameGems.ToString();
    }

    public void OnGameScoreChangedReceived()
    {
        Score.text = PlayerData.GameScore.ToString();
    }

    public void OnGameWaveChangedReceived()
    {
        Wave.text = PlayerData.GameWave.ToString();
    }

    public void OnShieldNeedChangedReceived()
    {
        if (PlayerData.ShieldNeed <= 0)
        {
            ShieldIcon.enabled = false;
            ShieldText.enabled = false;
            ShieldText.text = "0";
        }
        else
        {
            ShieldIcon.enabled = true;
            ShieldText.enabled = true;
            ShieldText.text = PlayerData.ShieldNeed.ToString();
        }
    }

    public override void EnableControls(bool enabled)
    {
        PauseButton.enabled = enabled;
        BarkButton.enabled = enabled;
    }

    public override void CloseMenu(float fade = 0)
    {
        Tools.Player = null;    // We are leaving the game and returning to main menu, so Tools.Player is no longer valid.
        PlayerData.OnGameGemsChanged -= OnGameGemsChangedReceived;
        PlayerData.OnGameScoreChanged -= OnGameScoreChangedReceived;
        PlayerData.OnShieldNeedChanged -= OnShieldNeedChangedReceived;
        PlayerData.OnGameWaveChanged -= OnGameWaveChangedReceived;
        base.CloseMenu(fade);
    }

    public override void OpenMenu(float fade = 0)
    {
        PlayerData.OnGameGemsChanged += OnGameGemsChangedReceived;
        PlayerData.OnGameScoreChanged += OnGameScoreChangedReceived;
        PlayerData.OnShieldNeedChanged += OnShieldNeedChangedReceived;
        PlayerData.OnGameWaveChanged += OnGameWaveChangedReceived;
        OnGameGemsChangedReceived();
        OnGameScoreChangedReceived();
        OnShieldNeedChangedReceived();
        OnGameWaveChangedReceived();
        base.OpenMenu(fade);
    }
}
