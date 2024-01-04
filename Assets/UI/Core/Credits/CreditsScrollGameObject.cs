using UnityEngine;
using UnityEngine.UI;

public class CreditsScrollGameObject : CreditsScrollObject
{
    public GameObject GO { get; set; }
    private Renderer ObjectRenderer { get; set; }

    public CreditsScrollGameObject(float spacing, GameObject go) : base(spacing)
    {
        GO = go;
        var rectTransform = GO.GetComponent<RectTransform>();
        Height = rectTransform != null ? rectTransform.rect.height : 0f;
    }

    public override void SetTopY(float position)
    {
        GO.transform.position = new Vector3(GO.transform.position.x, position + HalfHeight, GO.transform.position.z);
    }

    public override float GetBottomY()
    {
        return GO.transform.position.y - HalfHeight;
    }

    public override void SlideY(float slide)
    {
        GO.transform.position = new Vector3(GO.transform.position.x, GO.transform.position.y + slide, GO.transform.position.z);
    }

    public override void StartFadingIn()
    {
        ObjectRenderer = GO.GetComponent<Renderer>();
        if (ObjectRenderer != null)
        {
            CurrentFade = 0f;
            SetObjectFade();
        }
    }

    public override void FadeIn(float delta)
    {
        CurrentFade = Mathf.Min(1.0f, CurrentFade + delta);
        SetObjectFade();
    }

    private void SetObjectFade()
    {
        if (ObjectRenderer == null)
        {
            return;
        }

        var color = ObjectRenderer.material.color;
        color.a = CurrentFade;
        ObjectRenderer.material.color = color;
    }

    public override void DestroyLine()
    {
        GameObject.Destroy(GO);
        ObjectRenderer = null;
        GO = null;
    }
}
