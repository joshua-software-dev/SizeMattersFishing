using Dalamud.Interface.Windowing;
using ImGuiNET;
using SizeMattersFishing.Structs;
using SizeMattersFishingLib.Spearfishing;
using System.Numerics;
using System;


namespace SizeMattersFishing.GUI;

public class DalamudOverlayContainer
{
    public bool IsOpen
    {
        set
        {
            _row01Window.IsOpen = value;
            _row02Window.IsOpen = value;
            _row03Window.IsOpen = value;
        }
    }

    private readonly DalamudOverlayWindow _row01Window;
    private readonly DalamudOverlayWindow _row02Window;
    private readonly DalamudOverlayWindow _row03Window;
    
    public DalamudOverlayContainer(ISpearfishingData spearData)
    {
        _row01Window = new DalamudOverlayWindow("SpearFishingRow01", spearData, SpearfishingRow.Row01);
        _row02Window = new DalamudOverlayWindow("SpearFishingRow02", spearData, SpearfishingRow.Row02);
        _row03Window = new DalamudOverlayWindow("SpearFishingRow03", spearData, SpearfishingRow.Row03);
    }

    public void RegisterWindows(WindowSystem windowSystem)
    {
        windowSystem.AddWindow(_row01Window);
        windowSystem.AddWindow(_row02Window);
        windowSystem.AddWindow(_row03Window);
    }

    private unsafe SpearfishingWindowPosition? GetPositionInfo(IntPtr addonPtr)
    {
        var spearfishingAddon = (AddonSpearfishing*) addonPtr;
        return spearfishingAddon->AtkUnitBase.RootNode == null 
            ? null
            : new SpearfishingWindowPosition(
                spearfishingAddon->AtkUnitBase.RootNode->X, 
                spearfishingAddon->AtkUnitBase.RootNode->Y,
                spearfishingAddon->AtkUnitBase.RootNode->ScaleX,
                spearfishingAddon->AtkUnitBase.RootNode->ScaleY
            );
    }

    public void PrepareForRender(IntPtr addon)
    {
        var addonPosInfo = GetPositionInfo(addon);
        if (addonPosInfo == null)
        {
            return;
        }

        var fontScale = ImGui.GetIO().FontGlobalScale;
        var newWindowSize = new Vector2(50 * fontScale, 50 * fontScale);
        _row01Window.Size = newWindowSize;
        _row02Window.Size = newWindowSize;
        _row03Window.Size = newWindowSize;

        _row01Window.Position = new Vector2(
            addonPosInfo.Value.PosX - newWindowSize.X, 
            addonPosInfo.Value.PosY + (155 * addonPosInfo.Value.ScaleY)
        );

        _row02Window.Position = new Vector2(
            addonPosInfo.Value.PosX - newWindowSize.X, 
            addonPosInfo.Value.PosY + (230 * addonPosInfo.Value.ScaleY)
        );

        _row03Window.Position = new Vector2(
            addonPosInfo.Value.PosX - newWindowSize.X, 
            addonPosInfo.Value.PosY + (305 * addonPosInfo.Value.ScaleY)
        );
    }
}
