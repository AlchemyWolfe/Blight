using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsController : FullScreenMenuController
{
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
    private int NextSectionIdx = 0;
    private int NextEntryIdx = -1;
    private float NextLineCountdown = 0;

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
        Lines ??= new List<CreditsScrollObject>();
        NextSectionIdx = 0;
        NextEntryIdx = 0;
        NextLineCountdown = 0;
        CreateNextLine();
    }

    public void EndCredits()
    {
        Lines.Clear();
        if (CreditsFinale != null)
        {
            CreditsFinale.SetActive(true);
        }
    }

    public CreditsScrollTextObject CreateCreditsScrollTextObject(TMP_Text textPrefab, string textKey, float offset)
    {
        if (string.IsNullOrEmpty(textKey))
        {
            return null;
        }
        var tmpText = Instantiate<TMP_Text>(textPrefab, gameObject.transform);
        var startPoint = tmpText.transform.localPosition.y;
        var erasePoint = -startPoint;
        var spacing = offset;
        var line = new CreditsScrollTextObject(startPoint, erasePoint, spacing, tmpText);
        line.Text.text = TextUtil.TranslateTextKey(textKey);
        line.Text.ForceMeshUpdate();
        line.Text.transform.position = new Vector3(tmpText.transform.position.x, tmpText.transform.position.y - offset, tmpText.transform.position.z);
        Lines.Add(line);
        return line;
    }

    public CreditsScrollGameObject CreateCreditsScrollGameObject(GameObject goPrefab, float offset)
    {
        var go = Instantiate(goPrefab, gameObject.transform);
        var startPoint = go.transform.localPosition.y;
        var erasePoint = -startPoint;
        var spacing = offset;
        var line = new CreditsScrollGameObject(startPoint, erasePoint, spacing, go);
        go.transform.position = new Vector3(go.transform.position.x, go.transform.position.y - offset, go.transform.position.z);
        Lines.Add(line);
        return line;
    }

    public void CreateNextLine()
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
            CreateNextLine();
            return;
        }

        float height = 0;
        var entry = section.Entries[NextEntryIdx];
        if (!string.IsNullOrEmpty(entry.LeftKey))
        {
            var leftText = CreateCreditsScrollTextObject(LeftTextPrefab, entry.LeftKey, LineSpacing);
            height = leftText.GetHeight();
        }
        if (!string.IsNullOrEmpty(entry.RightKey))
        {
            var rightText = CreateCreditsScrollTextObject(RightTextPrefab, entry.RightKey, LineSpacing);
            height = Mathf.Max(height, rightText.GetHeight());
        }
        if (!string.IsNullOrEmpty(entry.HeaderKey))
        {
            var spacing = entry.Type == CreditsEntrySO.EntryType.LargeHeader ? LargeHeaderSpacing : SmallHeaderSpacing;
            var prefab = entry.Type == CreditsEntrySO.EntryType.LargeHeader ? LargeHeaderPrefab : SmallHeaderPrefab;
            var rightText = CreateCreditsScrollTextObject(prefab, entry.HeaderKey, spacing);
            height = Mathf.Max(height, rightText.GetHeight());
        }
        if (entry.LeftObject != null)
        {
            var leftObject = CreateCreditsScrollGameObject(entry.LeftObject, LineSpacing);
            height = Mathf.Max(height, leftObject.GetHeight());
        }
        if (entry.CenterObject != null)
        {
            var centerObject = CreateCreditsScrollGameObject(entry.CenterObject, LineSpacing);
            height = Mathf.Max(height, centerObject.GetHeight());
        }
        if (entry.RightObject != null)
        {
            var rightObject = CreateCreditsScrollGameObject(entry.RightObject, LineSpacing);
            height = Mathf.Max(height, rightObject.GetHeight());

            // If we're a BuyMeACoffee button, set the correct URL.
            if (!string.IsNullOrEmpty(entry.CoffeeName))
            {
                if (rightObject.GO.TryGetComponent<BuyMeACoffee>(out var coffee))
                {
                    coffee.CoffeeName = entry.CoffeeName;
                }
            }
        }
        height += LineSpacing;

        NextLineCountdown += height;
        NextEntryIdx++;
    }

    // Update is called once per frame
    public void Update()
    {
        // Early out
        if (Lines.Count <= 0)
        {
            return;
        }

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

        var offset = Time.deltaTime * Speed * speedmult;
        // Move all the lines
        for (var i = Lines.Count - 1; i >= 0; i--)
        {
            var line = Lines[i];
            line.SlideY(offset);
            var newY = line.GetY();
            if (newY > line.ErasePoint)
            {
                Lines.Remove(line);
            }
        }

        NextLineCountdown -= offset;
        if (NextLineCountdown < 0f)
        {
            CreateNextLine();
        }
        if (Lines.Count <= 0)
        {
            EndCredits();
        }
    }
}
