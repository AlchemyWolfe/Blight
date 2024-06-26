using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonImageColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler
{
    public Image ButtonImage;
    public Color NormalColor;
    public Color HighlightedColor;
    public Color PressedColor;
    public Color DisabledColor;
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
        ButtonImage = TextButton.GetComponent<Image>();
    }

    private void Start()
    {
        SetHighlight(false);
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

    private void SetImageColor(Color color)
    {
        ButtonImage.color = color;
    }

    // Called when the button is clicked
    public void OnPointerDown(PointerEventData eventData)
    {
        if (TextButton.enabled == false)
        {
            SetImageColor(DisabledColor);
            return;
        }
        SetHighlight(false);
        SetImageColor(PressedColor);
        if (Audio != null && ClickSound != null)
        {
            Audio.PlayOneShot(ClickSound);
        }
    }

    private void SetHighlight(bool highlighted)
    {
        if (TextButton.enabled == false)
        {
            SetImageColor(DisabledColor);
            return;
        }
        if (highlighted && TextButton.enabled && !Highlighted)
        {
            Highlighted = true;
            SetImageColor(HighlightedColor);
            if (Audio != null && HoverSound != null)
            {
                Audio.PlayOneShot(HoverSound);
            }
        }
        if (!highlighted)
        {
            SetImageColor(NormalColor);
            Highlighted = false;
        }
    }

    private void OnDisable()
    {
        SetImageColor(DisabledColor);
        Highlighted = false;
    }

    private void OnEnable()
    {
        SetImageColor(NormalColor);
        Highlighted = false;
    }
}
