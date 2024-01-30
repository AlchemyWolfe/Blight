using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler
{
    public TMP_Text Text;
    public Color NormalColor;
    public Color HighlightedColor;
    public Color PressedColor;
    public AudioSource Audio;
    public AudioClip HoverSound;
    public AudioClip ClickSound;

    private Button TextButton;
    private Graphic ObjectGraphic;
    private bool Highlighted;

    void Start()
    {
        TextButton = GetComponent<Button>();
        if (Text != null)
        {
            ObjectGraphic = Text.GetComponent<Graphic>();
        }
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

    // Called when the button is clicked
    public void OnPointerDown(PointerEventData eventData)
    {
        SetHighlight(false);
        if (ObjectGraphic != null)
        {
            ObjectGraphic.color = PressedColor;
        }
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
            if (ObjectGraphic != null)
            {
                ObjectGraphic.color = HighlightedColor;
            }
            if (Audio != null && HoverSound != null)
            {
                Audio.PlayOneShot(HoverSound);
            }
        }
        if (!highlighted)
        {
            if (ObjectGraphic != null)
            {
                ObjectGraphic.color = NormalColor;
            }
            Highlighted = false;
        }
    }
}
