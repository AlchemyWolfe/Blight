using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsScrollTextObject : CreditsScrollObject
{
    public TMP_Text Text { get; set; }
    private Graphic ObjectGraphic { get; set; }

    public CreditsScrollTextObject(float spacing, TMP_Text tmpText, string text) : base(spacing)
    {
        Text = tmpText;
        Text.text = text;
        Text.ForceMeshUpdate();
        Height = Text.textBounds.size.y;
    }

    public override void SetTopY(float position)
    {
        Text.transform.position = new Vector3(Text.transform.position.x, position - HalfHeight, Text.transform.position.z);
    }

    public override float GetBottomY()
    {
        return Text.transform.position.y - HalfHeight;
    }

    public override void SlideY(float slide)
    {
        Text.transform.position = new Vector3(Text.transform.position.x, Text.transform.position.y + slide, Text.transform.position.z);
    }

    public override void StartFadingIn()
    {
        ObjectGraphic = Text.GetComponent<Graphic>();
        if (ObjectGraphic != null)
        {
            CurrentFade = 0f;
            SetTextFade();
        }
    }

    public override void FadeIn(float delta)
    {
        CurrentFade = Mathf.Min(1.0f, CurrentFade + delta);
        SetTextFade();
    }

    private void SetTextFade()
    {
        if (ObjectGraphic == null)
        {
            return;
        }

        var color = ObjectGraphic.color;
        color.a = CurrentFade;
        ObjectGraphic.color = color;
    }

    public override void DestroyLine()
    {
        GameObject.Destroy(Text);
        ObjectGraphic = null;
        Text = null;
    }
}

