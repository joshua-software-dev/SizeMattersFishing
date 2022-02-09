namespace SizeMattersFishingLib.GUI;

public interface IGuiWindow : IUserInterface
{
    public bool IsOpen { get; set; }
    public void PreDraw();
    public void PostDraw();
}
