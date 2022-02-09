using ImGuiNET;
using SizeMattersFishingLib.GUI;
using System.Numerics;


namespace PluginUiTestbed.GUI;

public class MockOverlayWindow : IGuiWindow
{
    public string DrawValue
    {
        get => _overlayUi.DrawValue; 
        set => _overlayUi.DrawValue = value;
    }
    public bool IsOpen { get; set; }
    public Vector2 Position { get; set; }

    private Vector2 StartWindowSize { get; }

    private readonly string _chosenHeader;
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

    public MockOverlayWindow(string header)
    {
        _overlayUi = new OverlayUserInterface();

        var fontScale = ImGui.GetIO().FontGlobalScale;
        StartWindowSize = new Vector2(50 * fontScale, 50 * fontScale);

        _chosenHeader = header;
        IsOpen = false;
    }

    public void PreDraw()
    {
        if (IsOpen)
        {
            var shouldDeltaDetailDraw = true;
            ImGui.SetNextWindowSize(StartWindowSize, ImGuiCond.FirstUseEver);
            if (ImGui.Begin(_chosenHeader, ref shouldDeltaDetailDraw, WindowFlagsVal))
            {
                ImGui.SetWindowPos(Position);
            }
        }
    }

    public void Draw()
    {
        if (IsOpen)
        {
            _overlayUi.Draw();
        }
    }

    public void PostDraw()
    {
        if (IsOpen)
        {
            ImGui.End();
        }
    }
}
