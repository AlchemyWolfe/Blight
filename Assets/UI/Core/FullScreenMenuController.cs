using System;
using UnityEngine;

public class FullScreenMenuController : MonoBehaviour
{
    [SerializeField]
    public BackgroundController background;
    public virtual FullscreenMenuType Type { get => FullscreenMenuType.None; }

    public Action PauseToggleRequested { get; set; }
    public Action<FullscreenMenuType> MenuChangeRequested { get; set; }
    public Action<FullScreenMenuController> OnFinishedClosing { get; set; }
    public Action<FullScreenMenuController> OnFinishedOpening { get; set; }

    public virtual void EnableControls(bool enabled)
    {
    }

    public virtual void CloseMenu(float fade = 0f)
    {
        OnFinishedClosing?.Invoke(this);
    }

    public virtual void OpenMenu(float fade = 0f)
    {
        OnFinishedOpening?.Invoke(this);
    }
}
