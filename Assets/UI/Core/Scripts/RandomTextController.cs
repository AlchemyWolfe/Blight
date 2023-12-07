using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomTextController : MonoBehaviour
{
    public static float ReadingTimePerCharacter = 0.0625f;  // Assuming 16 characters per second.

    [SerializeField]
    private TMP_Text _textField;
    public TMP_Text TextField => _textField;

    [SerializeField]
    private RandomTextSO _randomText;
    public RandomTextSO RandomText => _randomText;

    [SerializeField]
    private Image _readingProgress;
    public Image ReadingProgress => _readingProgress;

    [SerializeField]
    [Tooltip("Higher values make the text stay around longer.")]
    private FloatRange _readingSpeed;
    public FloatRange ReadingSpeed => _readingSpeed;

    public bool ShowingNonRandomText { get; set; }
    private bool Paused { get; set; }
    private float NextTextUpdate { get; set; }
    private float LastTextUpdate { get; set; }

    void Awake()
    {
        if (TextField != null)
        {
            TextField.text = string.Empty;
        }
    }

    public void ShowNextRandomText()
    {
        if (RandomText == null)
        {
            NextTextUpdate = 0;
            LastTextUpdate = 0;
            return;
        }

        var effectiveReadingSpeed = ReadingSpeed.Max > 0f ? Random.Range(ReadingSpeed.Min, ReadingSpeed.Max) : 1f;

        TextField.text = RandomText.GetRandomText();
        NextTextUpdate = Time.time + TextField.text.Length * effectiveReadingSpeed * ReadingTimePerCharacter;
        LastTextUpdate = Time.time;
    }

    public void Pause(bool pause = true)
    {
        Paused = pause;
    }

    void Update()
    {
        if (Paused || ShowingNonRandomText || TextField == null || RandomText == null)
            return;

        if (Time.time > NextTextUpdate)
        {
            ShowNextRandomText();
        }

        if (ReadingProgress != null)
        {
            var progress = MathUtil.GetTimeProgress(LastTextUpdate, NextTextUpdate);
            ReadingProgress.fillAmount = progress;
        }
    }

    public void ShowNonRandomText(string textKey)
    {
        TextField.text = TextUtil.TranslateTextKey(textKey);
        NextTextUpdate = Time.time;
        ShowingNonRandomText = true;
    }

    public void UpdateNonRandomTextProgress(float progress)
    {
        ReadingProgress.fillAmount = progress;
        if (progress >= 1f)
        {
            ShowingNonRandomText = false;
        }
    }
}
