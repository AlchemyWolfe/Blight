using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MagicChoiceButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private PlayerDataSO _playerData;
    public PlayerDataSO PlayerData { get => _playerData; set => _playerData = value; }

    [SerializeField]
    private Image _selectedIcon;
    public Image SelectedIcon => _selectedIcon;

    public int ChoiceIdx;

    void Start()
    {
        UpdateSelected();
        PlayerData.OnMagicChoiceChanged += UpdateSelected;
    }

    private void UpdateSelected()
    {
        SelectedIcon.enabled = PlayerData.ChosenMagic == ChoiceIdx;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PlayerData.ChosenMagic = ChoiceIdx;
    }
}
