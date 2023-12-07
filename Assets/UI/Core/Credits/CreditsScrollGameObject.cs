using UnityEngine;

public class CreditsScrollGameObject : CreditsScrollObject
{
    public GameObject GO { get; set; }

    public CreditsScrollGameObject(float startPoint, float erasePoint, float spacing, GameObject go) : base(startPoint, erasePoint, spacing)
    {
        GO = go;
    }

    public override float GetHeight()
    {
        var rectTransform = GO.GetComponent<RectTransform>();
        return (rectTransform != null ? rectTransform.rect.height : 0f) + Spacing;
    }

    public override float GetY()
    {
        return GO.transform.localPosition.y;
    }

    public override void SlideY(float slide)
    {
        GO.transform.localPosition = new Vector3(GO.transform.localPosition.x, GO.transform.localPosition.y + slide, GO.transform.localPosition.z);
    }
}
