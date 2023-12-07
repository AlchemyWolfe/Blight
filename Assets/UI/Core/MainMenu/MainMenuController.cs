using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : FullScreenMenuController
{
    [SerializeField]
    private Button _continueButton;
    public Button ContinueButton => _continueButton;
    [SerializeField]
    private Button _newGameButton;
    public Button NewGameButton => _newGameButton;
    [SerializeField]
    private Button _creditsButton;
    public Button CreditsButton => _creditsButton;
    [SerializeField]
    private Button _optionsButton;
    public Button OptionsButton => _optionsButton;
    [SerializeField]
    private string _gameScene;
    public string GameScene => _gameScene;

    public override FullscreenMenuType Type { get => FullscreenMenuType.MainMenu; }
    //private SavegameEntry ContinueSave { get; set; }

    void Start()
    {
        ContinueButton.onClick.AddListener(OnContinueButtonClicked);
        NewGameButton.onClick.AddListener(OnNewGameButtonClicked);
        CreditsButton.onClick.AddListener(OnCreditsButtonClicked);
        OptionsButton.onClick.AddListener(OnOptionsButtonClicked);

        FindExistingGame();
        ContinueButton.gameObject.SetActive(false);// ContinueSave != null);
        NewGameButton.gameObject.SetActive(true);// ContinueSave == null);
    }

    public override void EnableControls(bool enabled)
    {
        ContinueButton.enabled = enabled;
        NewGameButton.enabled = enabled;
        CreditsButton.enabled = enabled;
        OptionsButton.enabled = enabled;
    }

    private void FindExistingGame()
    {
    }

    private void StartGame()
    {
        /*
        if (ContinueSave == null)
        {
            ContinueSave = new SavegameEntry();
            ContinueSave.SceneName = "BreakableDungeon";
        }
        LoadGameRequested?.Invoke(ContinueSave);
        */
        SceneChangeRequested?.Invoke(GameScene);
    }

    private void OnContinueButtonClicked()
    {
        // TODO: Add loading the game to the loading screen.
        StartGame();
    }

    private void OnNewGameButtonClicked()
    {
        StartGame();
    }

    private void OnCreditsButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.Credits);
    }

    private void OnOptionsButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.Options);
    }
}
