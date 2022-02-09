using ImGuiNET;


namespace SizeMattersFishingLib.GUI;

public class OverlayUserInterface : IUserInterface
{
    public string DrawValue { get; set; } = string.Empty;

    public void Draw()
    {
        ImGui.Text(DrawValue);
    }
}
