public class CreditsScrollObject
{
    public float StartPoint { get; set; }
    public float ErasePoint { get; set; }
    public float Spacing { get; set; }

    public CreditsScrollObject(float startPoint, float erasePoint, float spacing)
    {
        StartPoint = startPoint;
        ErasePoint = erasePoint;
        Spacing = spacing;
    }

    public virtual float GetHeight()
    {
        return Spacing;
    }

    public virtual float GetY()
    {
        return 0f;
    }

    public virtual void SlideY(float slide)
    {
    }
}
