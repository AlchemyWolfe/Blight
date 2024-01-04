using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsController : FullScreenMenuController
{
    public enum DisplayStage
    {
        Title,
        FadingLinesIn,
        Scrolling,
        ScrollingLastLines,
        EndMessage
    }

    [SerializeField]
    private Button _closeButton;
    public Button CloseButton => _closeButton;

    [SerializeField]
    private CreditsListSO _creditsList;
    public CreditsListSO CreditsList => _creditsList;

    [SerializeField]
    private GameObject _creditsFinale;
    public GameObject CreditsFinale => _creditsFinale;

    [SerializeField]
    private TMP_Text _largeSectionHeaderPrefab;
    public TMP_Text LargeHeaderPrefab => _largeSectionHeaderPrefab;

    [SerializeField]
    private TMP_Text _sectionHeaderPrefab;
    public TMP_Text SmallHeaderPrefab => _sectionHeaderPrefab;

    [SerializeField]
    private TMP_Text _leftTextPrefab;
    public TMP_Text LeftTextPrefab => _leftTextPrefab;

    [SerializeField]
    private TMP_Text _rightTextPrefab;
    public TMP_Text RightTextPrefab => _rightTextPrefab;

    [SerializeField]
    private float _speed;
    public float Speed => _speed;

    [SerializeField]
    private float _startOffsetFromTop;
    public float StartOffsetFromTop => _startOffsetFromTop;

    [SerializeField]
    private float _fadeInDuration;
    public float FadeInDuration => _fadeInDuration;

    [SerializeField]
    private float _largeSectionSpacing;
    public float LargeHeaderSpacing => _largeSectionSpacing;

    [SerializeField]
    private float _sectionSpacing;
    public float SmallHeaderSpacing => _sectionSpacing;

    [SerializeField]
    private float _lineSpacing;
    public float LineSpacing => _lineSpacing;

    public override FullscreenMenuType Type { get => FullscreenMenuType.Credits; }
    private List<CreditsScrollObject> Lines { get; set; }
    private List<CreditsScrollObject> LinesFadingIn { get; set; }
    private int NextSectionIdx = 0;
    private int NextEntryIdx = -1;
    private float NextLineY = 0f;
    private DisplayStage Stage = DisplayStage.Title;
    private float ScreenTop = 100f;
    private float ScreenBottom = -100f;

    public override void EnableControls(bool enabled)
    {
        CloseButton.enabled = enabled;
    }

    public void Start()
    {
        if (CreditsFinale != null)
        {
            CreditsFinale.SetActive(false);
        }
    }

    private void OnCloseButtonClicked()
    {
        MenuChangeRequested?.Invoke(FullscreenMenuType.MainMenu);
    }

    public override void CloseMenu(float fade = 0)
    {
        LinesFadingIn.Clear();
        foreach (var line in Lines)
        {
            line.DestroyLine();
        }
        Lines.Clear();
        CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        base.CloseMenu(fade);
    }

    public override void OpenMenu(float fade = 0)
    {
        if (CreditsFinale != null)
        {
            CreditsFinale.SetActive(false);
        }
        CloseButton.onClick.AddListener(OnCloseButtonClicked);
        StartCredits();
        base.OpenMenu(fade);
    }

    public void StartCredits()
    {
        ScreenTop = Screen.height;
        ScreenBottom = 0;
        Stage = DisplayStage.Title;
        Lines ??= new List<CreditsScrollObject>();
        LinesFadingIn ??= new List<CreditsScrollObject>();
        NextSectionIdx = 0;
        NextEntryIdx = 0;
        NextLineY = ScreenTop - StartOffsetFromTop;
        CreateNextLine(NextLineY);
    }

    public void ShowFinale()
    {
        if (CreditsFinale != null)
        {
            CreditsFinale.SetActive(true);
        }
    }

    public CreditsScrollTextObject CreateCreditsScrollTextObject(TMP_Text textPrefab, string textKey, float position, float spacing)
    {
        if (string.IsNullOrEmpty(textKey))
        {
            return null;
        }
        var tmpText = Instantiate<TMP_Text>(textPrefab, gameObject.transform);
        var text = TextUtil.TranslateTextKey(textKey);
        var line = new CreditsScrollTextObject(spacing, tmpText, text);
        line.SetTopY(position);
        Lines.Add(line);
        if (Stage == DisplayStage.FadingLinesIn)
        {
            line.StartFadingIn();
            LinesFadingIn.Add(line);
        }
        return line;
    }

    public CreditsScrollGameObject CreateCreditsScrollGameObject(GameObject goPrefab, string coffeeName, float position, float spacing)
    {
        var go = Instantiate(goPrefab, gameObject.transform);
        var line = new CreditsScrollGameObject(spacing, go);
        line.SetTopY(position);
        Lines.Add(line);
        if (Stage == DisplayStage.FadingLinesIn)
        {
            line.StartFadingIn();
            LinesFadingIn.Add(line);
        }
        // If we're a BuyMeACoffee button, set the correct URL.
        if (!string.IsNullOrEmpty(coffeeName))
        {
            if (go.TryGetComponent<BuyMeACoffee>(out var coffee))
            {
                coffee.CoffeeName = coffeeName;
            }
        }
        return line;
    }

    public void CreateNextLine(float position)
    {
        if (CreditsList.Sections.Count <= NextSectionIdx)
        {
            // We have no more lines to make.
            return;
        }

        var section = CreditsList.Sections[NextSectionIdx];
        if (section.Entries.Count <= NextEntryIdx)
        {
            // Proceed to the next section.
            ++NextSectionIdx;
            NextEntryIdx = 0;
            CreateNextLine(position);
            return;
        }

        float totalHeight = 0;
        var entry = section.Entries[NextEntryIdx];
        if (!string.IsNullOrEmpty(entry.LeftKey))
        {
            var leftText = CreateCreditsScrollTextObject(LeftTextPrefab, entry.LeftKey, position, LineSpacing);
            totalHeight = leftText.Height + LineSpacing;
        }
        if (!string.IsNullOrEmpty(entry.RightKey))
        {
            var rightText = CreateCreditsScrollTextObject(RightTextPrefab, entry.RightKey, position, LineSpacing);
            totalHeight = Mathf.Max(totalHeight, rightText.Height + LineSpacing);
        }
        if (!string.IsNullOrEmpty(entry.HeaderKey))
        {
            var spacing = entry.Type == CreditsEntrySO.EntryType.LargeHeader ? LargeHeaderSpacing : SmallHeaderSpacing;
            var prefab = entry.Type == CreditsEntrySO.EntryType.LargeHeader ? LargeHeaderPrefab : SmallHeaderPrefab;
            var rightText = CreateCreditsScrollTextObject(prefab, entry.HeaderKey, position, spacing);
            totalHeight = Mathf.Max(totalHeight, rightText.Height + spacing);
        }
        if (entry.LeftObject != null)
        {
            var leftObject = CreateCreditsScrollGameObject(entry.LeftObject, entry.CoffeeName, position, LineSpacing);
            totalHeight = Mathf.Max(totalHeight, leftObject.Height + LineSpacing);
        }
        if (entry.CenterObject != null)
        {
            var centerObject = CreateCreditsScrollGameObject(entry.CenterObject, entry.CoffeeName, position, LineSpacing);
            totalHeight = Mathf.Max(totalHeight, centerObject.Height + LineSpacing);
        }
        if (entry.RightObject != null)
        {
            var rightObject = CreateCreditsScrollGameObject(entry.RightObject, entry.CoffeeName, position, LineSpacing);
            totalHeight = Mathf.Max(totalHeight, rightObject.Height + LineSpacing);
        }
        if (entry.Type == CreditsEntrySO.EntryType.Spacer)
        {
            totalHeight = Mathf.Max(totalHeight, entry.SpacerHeight + LineSpacing);
        }

        NextLineY -= totalHeight;
        NextEntryIdx++;
    }

    public void Update()
    {
        // Early out
        if (Lines.Count <= 0)
        {
            return;
        }

        var minAlpha = 1f;
        if (LinesFadingIn.Count > 0)
        {
            minAlpha = FadeInCredits();
        }

        switch (Stage)
        {
            case DisplayStage.Title:
                Stage = DisplayStage.FadingLinesIn;
                break;

            case DisplayStage.FadingLinesIn:
                if (minAlpha > 0.5f)
                {
                    CreateNextLine(NextLineY);
                }
                if (NextLineY < ScreenBottom || CreditsList.Sections.Count <= NextSectionIdx)
                {
                    Stage = DisplayStage.Scrolling;
                }
                break;

            case DisplayStage.Scrolling:
                ScrollCredits();
                if (NextLineY > ScreenBottom - LineSpacing)
                {
                    CreateNextLine(NextLineY);
                }
                if (CreditsList.Sections.Count <= NextSectionIdx)
                {
                    // We have no more lines to make.
                    Stage = DisplayStage.ScrollingLastLines;
                }
                break;

            case DisplayStage.ScrollingLastLines:
                ScrollCredits();
                if (NextLineY > ScreenTop * 0.75f)
                {
                    // We have no more lines to make.
                    Stage = DisplayStage.ScrollingLastLines;
                    ShowFinale();
                }
                break;

            case DisplayStage.EndMessage:
                if (Lines.Count > 0)
                {
                    ScrollCredits();
                }
                break;
        }
    }

    public float FadeInCredits()
    {
        var delta = Time.deltaTime / FadeInDuration;
        var minAlpha = 1.0f;
        for (var i = LinesFadingIn.Count-1; i >= 0; --i)
        {
            var line = LinesFadingIn[i];
            line.FadeIn(delta);
            minAlpha = Mathf.Min(minAlpha, line.CurrentFade);
            if (line.CurrentFade >= 1f)
            {
                LinesFadingIn.RemoveAt(i);
            }
        }
        return minAlpha;
    }

    public void ScrollCredits()
    {
        // Adjust player controlled speed
        float speedmult = 1;
        if (Input.touchCount > 0)
        {
            for (var i = 0; i < Input.touchCount; i++)
            {
                speedmult *= 2f;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Space))
            {
                speedmult *= 2f;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speedmult *= 2f;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                speedmult *= 2f;
            }
        }

        // Move all the lines
        var offset = Time.deltaTime * Speed * speedmult;
        for (var i = Lines.Count - 1; i >= 0; i--)
        {
            var line = Lines[i];
            line.SlideY(offset);
            if (line.GetBottomY() > ScreenTop)
            {
                Lines.Remove(line);
            }
        }

        // Check for adding the next line
        NextLineY += offset;
    }
}
