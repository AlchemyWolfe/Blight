using UnityEngine;
using UnityEngine.UI;

// This is meant to be dropped on graphic elements that you want to fade in automatically.
// There is support if you want to call FadeIn and FadeOut directly.
public class ObjectFadeIn : MonoBehaviour
{
    public enum FadeState
    {
        Done,
        FadingIn,
        FadingOut,
        NoGraphic
    }

    [SerializeField]
    private float _startAlpha = 0f;
    public float StartAlpha => _startAlpha;
    [SerializeField]
    private float _endAlpha = 1f;
    public float EndAlpha => _endAlpha;
    [SerializeField]
    private float _delay;
    public float Delay => _delay;
    [SerializeField]
    private float _duration = 0.1f;
    public float Duration => _duration;
    [SerializeField]

    private Graphic ObjectGraphic { get; set; }
    private float StartTime { get; set; }
    private float EndTime { get; set; }
    private FadeState State { get; set; }

    void Awake()
    {
        ObjectGraphic = GetComponent<Graphic>();

        if (ObjectGraphic == null)
        {
            State = FadeState.NoGraphic;
            return;
        }

        var color = ObjectGraphic.color;
        color.a = StartAlpha;
        ObjectGraphic.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case FadeState.FadingIn:
                var color = ObjectGraphic.color;
                var progress = MathUtil.GetTimeProgress(StartTime, EndTime);
                if (progress <= 0)
                {
                    color.a = StartAlpha;
                }
                else if (progress >= 1)
                {
                    color.a = EndAlpha;
                }
                else
                {
                    color.a = StartAlpha + (EndAlpha - StartAlpha) * progress;
                }
                ObjectGraphic.color = color;
                break;

            case FadeState.FadingOut:
                color = ObjectGraphic.color;
                progress = MathUtil.GetTimeProgress(StartTime, EndTime);
                if (progress <= 0)
                {
                    color.a = EndAlpha;
                }
                else if (progress >= 1)
                {
                    color.a = StartAlpha;
                }
                else
                {
                    color.a = EndAlpha + (StartAlpha - EndAlpha) * progress;
                }
                ObjectGraphic.color = color;
                break;
        }
    }

    private void OnEnable()
    {
        FadeIn();
    }

    public void FadeIn(float delayOverride = 0f)
    {
        State = FadeState.FadingIn;

        var color = ObjectGraphic.color;
        color.a = StartAlpha;
        ObjectGraphic.color = color;

        var delay = delayOverride > 0f ? delayOverride : Delay;
        StartTime = Time.time + delay;
        EndTime = StartTime + Duration;
    }

    public void FadeOut(float delayOverride = 0f)
    {
        State = FadeState.FadingOut;

        var color = ObjectGraphic.color;
        color.a = EndAlpha;
        ObjectGraphic.color = color;

        var delay = delayOverride > 0f ? delayOverride : Delay;
        StartTime = Time.time + delay;
        EndTime = StartTime + Duration;
    }
}
