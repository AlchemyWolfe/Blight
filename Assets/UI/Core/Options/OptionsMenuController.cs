using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuController : FullScreenMenuController
{
    [SerializeField]
    private Button _closeButton;
    public Button CloseButton => _closeButton;

    [SerializeField]
    private Button _musicCheckbox;
    public Button MusicCheckbox => _musicCheckbox;

    [SerializeField]
    private Button _pantCheckbox;
    public Button PantCheckbox => _pantCheckbox;

    [SerializeField]
    private Button _powerUpCheckbox;
    public Button PowerUpCheckbox => _powerUpCheckbox;

    [SerializeField]
    private Button _shieldCheckbox;
    public Button ShieldCheckbox => _shieldCheckbox;

    [SerializeField]
    private Button _gemCheckbox;
    public Button GemCheckbox => _gemCheckbox;

    public GameOptionsSO Options;

    private ButtonTextColor MusicCheck;
    private ButtonTextColor PantCheck;
    private ButtonTextColor PowerUpCheck;
    private ButtonTextColor ShieldCheck;
    private ButtonTextColor GemCheck;

    public override FullscreenMenuType Type { get => FullscreenMenuType.Options; }

    private void Awake()
    {
        MusicCheck = MusicCheckbox.GetComponent<ButtonTextColor>();
        PantCheck = PantCheckbox.GetComponent<ButtonTextColor>();
        PowerUpCheck = PowerUpCheckbox.GetComponent<ButtonTextColor>();
        ShieldCheck = ShieldCheckbox.GetComponent<ButtonTextColor>();
        GemCheck = GemCheckbox.GetComponent<ButtonTextColor>();
    }

    public override void EnableControls(bool enabled)
    {
        CloseButton.enabled = enabled;
        MusicCheckbox.enabled = enabled;
        PantCheckbox.enabled = enabled;
        PowerUpCheckbox.enabled = enabled;
        ShieldCheckbox.enabled = enabled;
        GemCheckbox.enabled = enabled;
    }

    private void OnCloseButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.MainMenu);
    }
    private void OnMusicCheckboxClicked()
    {
        MusicCheck.Checked = !Options.EnableMusic;
        Options.EnableMusic = !Options.EnableMusic;
    }

    private void OnPantCheckboxClicked()
    {
        PantCheck.Checked = !Options.EnablePant;
        Options.EnablePant = !Options.EnablePant;
    }

    private void OnPowerUpCheckboxClicked()
    {
        PowerUpCheck.Checked = !Options.ShowPowerupIndicators;
        Options.ShowPowerupIndicators = !Options.ShowPowerupIndicators;
    }

    private void OnShieldCheckboxClicked()
    {
        ShieldCheck.Checked = !Options.ShowShieldIndicators;
        Options.ShowShieldIndicators = !Options.ShowShieldIndicators;
    }

    private void OnGemCheckboxClicked()
    {
        GemCheck.Checked = !Options.ShowGemIndicators;
        Options.ShowGemIndicators = !Options.ShowGemIndicators;
    }

    public override void CloseMenu(float fade = 0)
    {
        CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        MusicCheckbox.onClick.RemoveListener(OnMusicCheckboxClicked);
        PantCheckbox.onClick.RemoveListener(OnPantCheckboxClicked);
        PowerUpCheckbox.onClick.RemoveListener(OnPowerUpCheckboxClicked);
        ShieldCheckbox.onClick.RemoveListener(OnShieldCheckboxClicked);
        GemCheckbox.onClick.RemoveListener(OnGemCheckboxClicked);
        base.CloseMenu(fade);
    }

    public override void OpenMenu(float fade = 0)
    {
        MusicCheck.Checked = Options.EnableMusic;
        PantCheck.Checked = Options.EnablePant;
        PowerUpCheck.Checked = Options.ShowPowerupIndicators;
        ShieldCheck.Checked = Options.ShowShieldIndicators;
        GemCheck.Checked = Options.ShowGemIndicators;
        CloseButton.onClick.AddListener(OnCloseButtonClicked);
        MusicCheckbox.onClick.AddListener(OnMusicCheckboxClicked);
        PantCheckbox.onClick.AddListener(OnPantCheckboxClicked);
        PowerUpCheckbox.onClick.AddListener(OnPowerUpCheckboxClicked);
        ShieldCheckbox.onClick.AddListener(OnShieldCheckboxClicked);
        GemCheckbox.onClick.AddListener(OnGemCheckboxClicked);
        base.OpenMenu(fade);
    }
}
