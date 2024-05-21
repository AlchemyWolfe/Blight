using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkinChoiceButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private PlayerDataSO _playerData;
    public PlayerDataSO PlayerData { get => _playerData; set => _playerData = value; }

    [SerializeField]
    private Image _selectedIcon;
    public Image SelectedIcon => _selectedIcon;

    public int ChoiceIdx;

    // Start is called before the first frame update
    void Start()
    {
        UpdateSelected();
        PlayerData.OnSkinChoiceChanged += UpdateSelected;
    }

    private void UpdateSelected()
    {
        SelectedIcon.enabled = PlayerData.ChosenSkin == ChoiceIdx;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PlayerData.ChosenSkin = ChoiceIdx;
    }
}
