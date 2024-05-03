using UnityEngine;
using UnityEngine.UI;

public class ResizeToFillScreen : MonoBehaviour
{
    public Image Background;

    private float backgroundWidth;
    private float backgroundHeight;
    private int lastScreenWidth;
    private int lastScreenHeight;

    // Start is called before the first frame update
    void Start()
    {
        Background.rectTransform.localScale = Vector3.one;
        backgroundWidth = Background.sprite.rect.size.x;
        backgroundHeight = Background.sprite.rect.size.y;
        ResizeBackground();
    }

    // Update is called once per frame
    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            ResizeBackground();
        }
    }

    void ResizeBackground()
    {
        // Get the size of the screen
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Calculate the aspect ratio of the screen
        float screenAspectRatio = screenWidth / screenHeight;

        // Get the aspect ratio of the background image
        float backgroundAspectRatio = backgroundWidth / backgroundHeight;

        // Calculate the scale factor
        float scaleFactor;

        if (screenAspectRatio > backgroundAspectRatio)
        {
            // Screen is wider than the background
            scaleFactor = screenWidth / backgroundWidth;
        }
        else
        {
            // Screen is taller than the background
            scaleFactor = screenHeight / backgroundHeight;
        }

        // Get the rect transform of the image
        RectTransform backgroundRectTransform = Background.rectTransform;

        // Set the size of the image to match the screen size
        backgroundRectTransform.sizeDelta = new Vector2(backgroundWidth * scaleFactor, backgroundHeight * scaleFactor);

        // You might also want to adjust the image position if necessary
        // backgroundRectTransform.anchoredPosition = Vector2.zero;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }
}
