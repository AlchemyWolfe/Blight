using UnityEngine;
using UnityEngine.UI;

public class ParallaxImage : MonoBehaviour
{
    [SerializeField]
    private Image _background;
    public Image Background => _background;

    [SerializeField]
    private float _speed;
    public float Speed => _speed;

    private Image BackgroundClone { get; set; }
    private float ImageWidth { get; set; }
    private float OriginalX { get; set; }
    private float LeftThreshold { get; set; }
    private bool ImageOnLeft { get; set; }

    // Start is called before the first frame update
    void Awake()
    {
        var rect = Background.rectTransform;
        ImageWidth = rect.rect.width;
        var position = Background.transform.position;
        OriginalX = position.x;
        LeftThreshold = OriginalX - ImageWidth;

        CreateBackgroundClone();
        MoveCloneToTheRight();
    }

    private void CreateBackgroundClone()
    {
        // Check if the Background reference is assigned
        if (Background != null)
        {
            // Create a new empty GameObject
            var cloneGO = new GameObject("BackgroundClone");
            cloneGO.transform.SetParent(Background.transform);

            // Add an Image component to the new GameObject
            BackgroundClone = cloneGO.AddComponent<Image>();

            // Copy properties from the original Image to the cloned Image component
            BackgroundClone.sprite = Background.sprite;
            BackgroundClone.color = Background.color;
            BackgroundClone.rectTransform.sizeDelta = Background.rectTransform.sizeDelta;
        }
    }

    private void MoveCloneToTheLeft()
    {
        BackgroundClone.transform.localPosition = new Vector3(-ImageWidth, 0f, 0f);
        ImageOnLeft = true;
    }

    private void MoveCloneToTheRight()
    {
        BackgroundClone.transform.localPosition = new Vector3(ImageWidth, 0f, 0f);
        ImageOnLeft = false;
    }

    // Update is called once per frame
    void Update()
    {
        var currentPosition = Background.transform.position;
        var newX = currentPosition.x - Time.deltaTime * Speed;
        if (newX < LeftThreshold)
        {
            newX += ImageWidth * 2;
            MoveCloneToTheLeft();
        }
        Background.transform.position = new Vector3(newX, currentPosition.y, currentPosition.z);
        if (ImageOnLeft && newX < OriginalX)
        {
            MoveCloneToTheRight();
        }

        // This is not required just for parallax, but the animation changes the image color for fading in.
        BackgroundClone.color = Background.color;
    }
}
