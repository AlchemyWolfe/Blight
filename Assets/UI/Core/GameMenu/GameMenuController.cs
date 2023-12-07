public class GameMenuController : FullScreenMenuController
{
    public override FullscreenMenuType Type { get => FullscreenMenuType.Game; }

    public override void EnableControls(bool enabled)
    {
    }

    public override void CloseMenu(float fade = 0)
    {
        base.CloseMenu(fade);
    }

    public override void OpenMenu(float fade = 0)
    {
        base.OpenMenu(fade);
    }
}
