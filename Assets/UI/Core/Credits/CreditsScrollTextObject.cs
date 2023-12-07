using TMPro;
using UnityEngine;

public class CreditsScrollTextObject : CreditsScrollObject
{
    public TMP_Text Text { get; set; }

    public CreditsScrollTextObject(float startPoint, float erasePoint, float spacing, TMP_Text text) : base(startPoint, erasePoint, spacing)
    {
        Text = text;
    }

    public override float GetHeight()
    {
        return Text.textBounds.size.y + Spacing;
    }

    public override float GetY()
    {
        return Text.transform.localPosition.y;
    }

    public override void SlideY(float slide)
    {
        Text.transform.localPosition = new Vector3(Text.transform.localPosition.x, Text.transform.localPosition.y + slide, Text.transform.localPosition.z);
    }
}

