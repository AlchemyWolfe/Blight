using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCellDetail : MonoBehaviour
{
    [SerializeField]
    private int _detailCost;
    public int DetailCost
    {
        get => _detailCost;
        set
        {
            _detailCost = value;
            if (DetailCostText != null)
            {
                DetailCostText.text = _detailCost.ToString();
            }
        }
    }
    [SerializeField]
    private TMP_Text DetailCostText;
    [SerializeField]
    private Button DetailUpButton;
    [SerializeField]
    private Button DetailDownButton;
    [SerializeField]
    private int _detailLevel;
    public int DetailLevel
    {
        get => _detailLevel;
        set
        {
            _detailLevel = value;
            if (DetailLevelText != null)
            {
                DetailLevelText.text = _detailLevel.ToString();
            }
        }
    }
    [SerializeField]
    private TMP_Text DetailLevelText;

    public int DetailLevelMin;
    public int DetailLevelMax = 5;

    [HideInInspector]
    public System.Action OnLevelChanged;

    private int InitialLevel;
    private bool Enabled;
    private ButtonImageColor UpButtonColor;
    private ButtonImageColor DownButtonColor;

    public int TotalCost
    {
        get
        {
            return _detailCost * (_detailLevel - DetailLevelMin);
        }
    }

    void Awake()
    {
        DetailUpButton.onClick.AddListener(DetailUpClicked);
        DetailDownButton.onClick.AddListener(DetailDownClicked);
        UpButtonColor = DetailUpButton.gameObject.GetComponent<ButtonImageColor>();
        DownButtonColor = DetailDownButton.gameObject.GetComponent<ButtonImageColor>();
    }

    private void DetailUpClicked()
    {
        if (DetailLevel < DetailLevelMax)
        {
            DetailLevel += 1;
            OnLevelChanged?.Invoke();
        }
        UpdateButtons();
    }

    private void DetailDownClicked()
    {
        if (DetailLevel > DetailLevelMin)
        {
            DetailLevel -= 1;
            OnLevelChanged?.Invoke();
        }
        UpdateButtons();
    }

    public void UpdateButtons()
    {
        DetailDownButton.enabled = Enabled && DetailLevel > DetailLevelMin;
        DetailUpButton.enabled = Enabled && DetailLevel < DetailLevelMax;
        UpButtonColor.enabled = DetailUpButton.enabled;
        DownButtonColor.enabled = DetailDownButton.enabled;
    }

    public void Initialize(int cost, int level, int min, int max, bool enabled = true)
    {
        DetailCost = cost;
        InitialLevel = level;
        DetailLevel = level;
        DetailLevelMin = min;
        DetailLevelMax = max;
        Enabled = enabled;
        UpdateButtons();
    }

    public void Reset()
    {
        DetailLevel = InitialLevel;

    }

    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
        UpdateButtons();
    }
}
