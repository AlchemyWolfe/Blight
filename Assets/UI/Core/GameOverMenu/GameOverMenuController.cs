using ContentAlchemy;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMenuController : FullScreenMenuController
{
    public GameSceneToolsSO Tools;

    [SerializeField]
    private PlayerDataSO _playerData;
    public PlayerDataSO PlayerData => _playerData;

    [SerializeField]
    private Button _closeButton;
    public Button CloseButton => _closeButton;

    [SerializeField]
    private Image _waveIcon;
    public Image WaveIcon => _waveIcon;

    [SerializeField]
    private TMP_Text _wave;
    public TMP_Text Wave => _wave;

    [SerializeField]
    private Image _scoreIcon;
    public Image ScoreIcon => _scoreIcon;

    [SerializeField]
    private TMP_Text _score;
    public TMP_Text Score => _score;

    [SerializeField]
    private Image _gemsIcon;
    public Image GemsIcon => _gemsIcon;

    [SerializeField]
    private TMP_Text _gems;
    public TMP_Text Gems => _gems;

    [SerializeField]
    private Button _exitButton;
    public Button ExitButton => _exitButton;

    public override FullscreenMenuType Type { get => FullscreenMenuType.GameOver; }

    public Sequence OpenSequence = null;

    private void OnCloseButtonClicked()
    {
        Tools.OnGameClose?.Invoke();
        MenuChangeRequested?.Invoke(FullscreenMenuType.MainMenu);
    }

    public override void EnableControls(bool enabled)
    {
        CloseButton.enabled = enabled;
        ExitButton.enabled = enabled;
    }

    public override void CloseMenu(float fade = 0)
    {
        CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        ExitButton.onClick.RemoveListener(OnCloseButtonClicked);
        Wave.text = string.Empty;
        Score.text = string.Empty;
        Gems.text = string.Empty;
        OpenSequence?.Kill();
        base.CloseMenu(fade);
    }

    public void WaveCallback()
    {
        WaveIcon.enabled = true;
        Wave.text = PlayerData.GameWave.ToString();
        if (PlayerData.PreviousHighestWave > 0 && PlayerData.HighestWave > PlayerData.PreviousHighestWave)
        {
            TMPRipple ripple = Wave.gameObject.GetComponent<TMPRipple>();
            if (ripple != null)
            {
                ripple.Reinit();
            }
        }
    }

    public void ScoreCallback()
    {
        ScoreIcon.enabled = true;
        var score = (int)PlayerData.GameScore;
        Score.text = score.ToString();
        if (PlayerData.PreviousHighScore <= 0f && PlayerData.HighScore > PlayerData.PreviousHighScore)
        {
            TMPRipple ripple = Score.gameObject.GetComponent<TMPRipple>();
            if (ripple != null)
            {
                ripple.Reinit();
            }
        }
    }

    public void GemsCallback()
    {
        GemsIcon.enabled = true;
        Gems.text = PlayerData.GameGems.ToString();
    }

    public override void OpenMenu(float fade = 0)
    {
        CloseButton.onClick.AddListener(OnCloseButtonClicked);
        ExitButton.onClick.AddListener(OnCloseButtonClicked);
        OpenSequence = DOTween.Sequence();
        if (OpenSequence != null)
        {
            WaveIcon.enabled = false;
            ScoreIcon.enabled = false;
            GemsIcon.enabled = false;
            float revealDelay = 0.33f;
            OpenSequence.PrependInterval(revealDelay);
            OpenSequence.AppendCallback(WaveCallback);
            OpenSequence.AppendInterval(revealDelay);
            OpenSequence.AppendCallback(ScoreCallback);
            OpenSequence.AppendInterval(revealDelay);
            OpenSequence.AppendCallback(GemsCallback);
            OpenSequence.OnKill(() => OpenSequence = null);
        }
        else
        {
            WaveIcon.enabled = true;
            ScoreIcon.enabled = true;
            GemsIcon.enabled = true;
            Wave.text = PlayerData.GameWave.ToString();
            Score.text = PlayerData.GameScore.ToString();
            Gems.text = PlayerData.GameGems.ToString();
        }
        base.OpenMenu(fade);
    }
}
