using Dalamud.Interface.Windowing;
using ImGuiNET;
using SizeMattersFishingLib.GUI;
using System.Numerics;


namespace SizeMattersFishing.GUI;

public class DalamudOverlayWindow : Window, IGuiWindow
{
    public string DrawValue
    {
        get => _overlayUi.DrawValue; 
        set => _overlayUi.DrawValue = value;
    }

    private readonly OverlayUserInterface _overlayUi;
    private const ImGuiWindowFlags WindowFlagsVal = ImGuiWindowFlags.NoNav |
                                                    ImGuiWindowFlags.NoNavFocus |
                                                    ImGuiWindowFlags.NoNavInputs |
                                                    ImGuiWindowFlags.NoResize |
                                                    ImGuiWindowFlags.NoScrollbar |
                                                    ImGuiWindowFlags.NoSavedSettings |
                                                    ImGuiWindowFlags.NoFocusOnAppearing |
                                                    ImGuiWindowFlags.NoDocking |
                                                    ImGuiWindowFlags.NoCollapse |
                                                    ImGuiWindowFlags.NoMove |
                                                    ImGuiWindowFlags.NoTitleBar;

    public DalamudOverlayWindow(string name) : base(name)
    {
        _overlayUi = new OverlayUserInterface();
        
        var fontScale = ImGui.GetIO().FontGlobalScale;
        var startWindowSize = new Vector2(50 * fontScale, 50 * fontScale);
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = startWindowSize,
            MaximumSize = startWindowSize
        };

        Flags = WindowFlagsVal;
        IsOpen = false;
    }

    public override void Draw() => 
        _overlayUi.Draw();
}
