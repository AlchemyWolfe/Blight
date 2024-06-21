using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler
{
    public TMP_Text Text;
    public Color NormalColor;
    public Color HighlightedColor;
    public Color PressedColor;
    public AudioSource Audio;
    public AudioClip HoverSound;
    public AudioClip ClickSound;

    [HideInInspector]
    public Button TextButton;

    private List<Graphic> ObjectGraphics;
    private bool Highlighted;

    void Awake()
    {
        TextButton = GetComponent<Button>();
        ObjectGraphics = new List<Graphic>();
        AddTextGraphic(Text);
    }

    public void AddTextGraphic(TMP_Text tmpText)
    {
        if (tmpText == null)
        {
            return;
        }
        ObjectGraphics.Add(tmpText.GetComponent<Graphic>());
    }

    // Called when the mouse pointer enters the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHighlight(true);
    }

    // Called when the mouse pointer exits the button
    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlight(false);
    }

    // Called when the button is selected (e.g., using keyboard navigation)
    public void OnSelect(BaseEventData eventData)
    {
        SetHighlight(true);
    }

    // Called when the button is deselected
    public void OnDeselect(BaseEventData eventData)
    {
        SetHighlight(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetHighlight(false);
    }

    private void SetTextColor(Color color)
    {
        foreach (var graphic in ObjectGraphics)
        {
            if (graphic != null)
            {
                graphic.color = color;
            }
        }
    }

    // Called when the button is clicked
    public void OnPointerDown(PointerEventData eventData)
    {
        SetHighlight(false);
        SetTextColor(PressedColor);
        if (Audio != null && ClickSound != null)
        {
            Audio.PlayOneShot(ClickSound);
        }
    }

    private void SetHighlight(bool highlighted)
    {
        if (highlighted && TextButton.enabled && !Highlighted)
        {
            Highlighted = true;
            SetTextColor(HighlightedColor);
            if (Audio != null && HoverSound != null)
            {
                Audio.PlayOneShot(HoverSound);
            }
        }
        if (!highlighted)
        {
            SetTextColor(NormalColor);
            Highlighted = false;
        }
    }

    private void OnEnable()
    {
        SetTextColor(NormalColor);
        Highlighted = false;
    }
}
