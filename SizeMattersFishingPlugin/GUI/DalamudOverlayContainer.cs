using Dalamud.Interface.Windowing;
using SizeMattersFishing.Structs;
using SizeMattersFishingLib.Spearfishing;
using System.Globalization;
using System.Numerics;
using System;


namespace SizeMattersFishing.GUI;

public class DalamudOverlayContainer
{
    private string Row01Value
    {
        get => _row01Window.DrawValue; 
        set => _row01Window.DrawValue = value;
    }

    private string Row02Value
    {
        get => _row02Window.DrawValue; 
        set => _row02Window.DrawValue = value;
    }

    private string Row03Value
    {
        get => _row03Window.DrawValue; 
        set => _row03Window.DrawValue = value;
    }

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
    private readonly ISpearfishingData _spearfishingData;
    
    public DalamudOverlayContainer(ISpearfishingData data)
    {
        _spearfishingData = data;
        _row01Window = new DalamudOverlayWindow("SpearFishingRow01");
        _row02Window = new DalamudOverlayWindow("SpearFishingRow02");
        _row03Window = new DalamudOverlayWindow("SpearFishingRow03");
    }

    public void RegisterWindows(WindowSystem windowSystem)
    {
        windowSystem.AddWindow(_row01Window);
        windowSystem.AddWindow(_row02Window);
        windowSystem.AddWindow(_row03Window);
    }

    public unsafe SpearfishingWindowPosition? GetPositionInfo(IntPtr addonPtr)
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
        var posInfo = GetPositionInfo(addon);
        if (posInfo == null) return;

        var deltaRow01 = _spearfishingData.FishPositions.FishPositionRow01XDelta;
        _ = _spearfishingData.FishPositions.FishPositionRow01;
        var deltaRow02 = _spearfishingData.FishPositions.FishPositionRow02XDelta;
        _ = _spearfishingData.FishPositions.FishPositionRow02;
        var deltaRow03 = _spearfishingData.FishPositions.FishPositionRow03XDelta;
        _ = _spearfishingData.FishPositions.FishPositionRow03;

        if (deltaRow01 == null || deltaRow02 == null || deltaRow03 == null) return;

        Row01Value = deltaRow01.Value.ToString("F3", CultureInfo.InvariantCulture);
        if (deltaRow01 != 0)
        {
            Row01Value += _spearfishingData.FishPositions.FishSizeRow01 switch
            {
                FishSize.Bottle => "\nB",
                FishSize.Small => "\nS",
                FishSize.Medium => "\nM",
                FishSize.Large => "\nL",
                _ => ""
            };
        }
        
        Row02Value = deltaRow02.Value.ToString("F3", CultureInfo.InvariantCulture);
        if (deltaRow02 != 0)
        {
            Row02Value += _spearfishingData.FishPositions.FishSizeRow02 switch
            {
                FishSize.Bottle => "\nB",
                FishSize.Small => "\nS",
                FishSize.Medium => "\nM",
                FishSize.Large => "\nL",
                _ => ""
            };
        }
        
        Row03Value = deltaRow03.Value.ToString("F3", CultureInfo.InvariantCulture);
        if (deltaRow03 != 0)
        {
            Row03Value += _spearfishingData.FishPositions.FishSizeRow03 switch
            {
                FishSize.Bottle => "\nB",
                FishSize.Small => "\nS",
                FishSize.Medium => "\nM",
                FishSize.Large => "\nL",
                _ => ""
            };
        }

        _row01Window.Position = new Vector2(posInfo.Value.PosX - 50, posInfo.Value.PosY + (155 * posInfo.Value.ScaleY));
        _row02Window.Position = new Vector2(posInfo.Value.PosX - 50, posInfo.Value.PosY + (230 * posInfo.Value.ScaleY));
        _row03Window.Position = new Vector2(posInfo.Value.PosX - 50, posInfo.Value.PosY + (305 * posInfo.Value.ScaleY));
    }
}
