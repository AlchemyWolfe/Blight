using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BarkButton : MonoBehaviour
{
    public GameObject Icon;
    public Image Fill;
    public GameSceneToolsSO Tools;
    public float CooldownTime = 5f;
    public AudioSource Audio;
    public AudioClip BarkActivateSound;
    public AudioClip BarkRestoreSound;

    public System.Action OnBarkButtonClicked { get; set; }

    private Button MyButton;
    private float Cooldown;

    private void Start()
    {
        Cooldown = 0f;
        Fill.fillAmount = 0f;
        MyButton = gameObject.GetComponent<Button>();
        MyButton.onClick.AddListener(OnButtonClicked);
    }

    private void Update()
    {
        if (Cooldown > 0f)
        {
            Fill.fillAmount = Cooldown / CooldownTime;
            Cooldown -= Time.deltaTime;
            if (Cooldown <= 0f)
            {
                Audio.PlayOneShot(BarkRestoreSound);
            }
        }
        else
        {
            Fill.fillAmount = 0f;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            OnButtonClicked();
        }
    }

    private void OnButtonClicked()
    {
        if (!Tools.IsPlayingGame || Tools.Player == null || Tools.Player.IsDying || !enabled)
        {
            return;
        }
        if (Cooldown <= 0f)
        {
            OnBarkButtonClicked?.Invoke();
            Cooldown = CooldownTime;
            Audio.PlayOneShot(BarkActivateSound);
            Tools.Player.Bark();
        }
    }
}
