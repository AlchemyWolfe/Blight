using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    public Animator Animator => _animator;

    [SerializeField, HorizontalGroup("AnimatorRow")]
    [ShowIf("Animator")]
    [Tooltip("If true, the background waits for the OpenAnimationComplete event before sending OnFinishedOpening.")]
    private bool _waitForOpen;
    public bool WaitForOpen => _waitForOpen;

    [SerializeField, HorizontalGroup("AnimatorRow")]
    [ShowIf("Animator")]
    [Tooltip("If true, the background waits for the CloseAnimationComplete event before sending OnFinishedHiding.")]
    private bool _waitForClose;
    public bool WaitForClose => _waitForClose;

    public Action<BackgroundController> OnFinishedHiding { get; set; }
    public Action<BackgroundController> OnFinishedShowing { get; set; }

    public virtual void ShowBackground()
    {
        if (Animator != null)
        {
            Animator.SetBool("Open", true);
            if (WaitForOpen)
            {
                return;
            }
        }
        OnFinishedShowing?.Invoke(this);
    }

    public virtual void HideBackground()
    {
        if (Animator != null)
        {
            Animator.SetBool("Open", false);
            if (WaitForClose)
            {
                return;
            }
        }
        OnFinishedHiding?.Invoke(this);
    }

    public void OpenAnimationComplete()
    {
        OnFinishedShowing?.Invoke(this);
    }

    public void CloseAnimationComplete()
    {
        OnFinishedHiding?.Invoke(this);
    }
}
