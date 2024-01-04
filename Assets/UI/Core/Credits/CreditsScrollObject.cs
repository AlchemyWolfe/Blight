public class CreditsScrollObject
{
    public float HalfHeight { get; set; }
    public float Spacing { get; set; }
    public float CurrentFade { get; set; }
    public float Height {
        get { return HalfHeight * 2f; }
        set { HalfHeight = value * 0.5f; }
    }

    public CreditsScrollObject(float spacing)
    {
        Spacing = spacing;
        CurrentFade = 0f;
    }

    public virtual void SetTopY(float position)
    {
    }

    public virtual float GetBottomY()
    {
        return 0f;
    }

    public virtual void SlideY(float slide)
    {
    }

    public virtual void StartFadingIn()
    {
    }

    public virtual void FadeIn(float delta)
    {
    }

    public virtual void DestroyLine()
    {
    }
}
