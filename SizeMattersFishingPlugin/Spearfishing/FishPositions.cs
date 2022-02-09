using Dalamud.Game.Gui;
using SizeMattersFishing.Structs;
using SizeMattersFishingLib.Spearfishing;
using System;


namespace SizeMattersFishing.Spearfishing;

internal enum SpearfishingRow
{
    Row01,
    Row02,
    Row03
}

public class FishPositions : IFishPositions
{
    private readonly GameGui _gameGui;

    public FishPositions(GameGui gameGui) => 
        _gameGui = gameGui;

    private (double X, double Y)? GetFishPosFromRow(SpearfishingRow row)
    {
        var addon = _gameGui.GetAddonByName("SpearFishing", 1);
        if (addon == IntPtr.Zero) return null;

        unsafe
        {
            var spearfishingAddon = (AddonSpearfishing*) addon;
            var position = row switch
            {
                SpearfishingRow.Row01 => 17,
                SpearfishingRow.Row02 => 16,
                SpearfishingRow.Row03 => 15,
                _ => throw new ArgumentOutOfRangeException(nameof(row), row, null)
            };

            var fish = spearfishingAddon->AtkUnitBase.UldManager.NodeList[position];
            return (fish->X, fish->Y);
        }
    }

    private double? GetFishPosXDeltaFromRow(SpearfishingRow row)
    {
        var previousX = row switch
        {
            SpearfishingRow.Row01 => _lastRow01Pos.X,
            SpearfishingRow.Row02 => _lastRow02Pos.X,
            SpearfishingRow.Row03 => _lastRow03Pos.X,
            _ => throw new ArgumentOutOfRangeException(nameof(row), row, null)
        };

        var currentPos = GetFishPosFromRow(row);
        if (currentPos == null) return null;
        var currentX = currentPos.Value.X;

        var xDelta = previousX > currentX 
            ? previousX - currentX // negative 
            : currentX - previousX; // positive

        if (xDelta is > 100 or < -100)
        {
            return 0;
        }

        return xDelta;
    }

    private FishSize? GetFishSizeFromRow(SpearfishingRow row)
    {
        var addon = _gameGui.GetAddonByName("SpearFishing", 1);
        if (addon == IntPtr.Zero) return null;

        unsafe
        {
            var spearfishingAddon = (AddonSpearfishing*) addon;
            var position = row switch
            {
                SpearfishingRow.Row01 => 17,
                SpearfishingRow.Row02 => 16,
                SpearfishingRow.Row03 => 15,
                _ => throw new ArgumentOutOfRangeException(nameof(row), row, null)
            };

            var fishContainer = spearfishingAddon->AtkUnitBase.UldManager.NodeList[position];
            if (!fishContainer->IsVisible) return null;

            var fishContainerNodeList = fishContainer->GetAsAtkComponentNode()->Component->UldManager.NodeList;
            for (var i = 0; i < 7; i++)
            {
                var fish = fishContainerNodeList[i];
                if (fish->IsVisible)
                {
                    return i switch
                    {
                        0 => FishSize.Bottle,
                        1 => FishSize.Small,
                        2 => FishSize.Medium,
                        3 => FishSize.Large,
                        4 => FishSize.Small,
                        5 => FishSize.Medium,
                        6 => FishSize.Large,
                        _ => null
                    };
                }
            }

            return null;
        }
    }

    private (double X, double Y) _lastRow01Pos = (0, 0);
    public (double X, double Y)? FishPositionRow01
    {
        get
        {
            var pos = GetFishPosFromRow(SpearfishingRow.Row01);
            if (pos != null)
            {
                _lastRow01Pos = pos.Value;
            }

            return pos;
        }
    }

    private (double X, double Y) _lastRow02Pos = (0, 0);
    public (double X, double Y)? FishPositionRow02
    {
        get
        {
            var pos = GetFishPosFromRow(SpearfishingRow.Row02);
            if (pos != null)
            {
                _lastRow02Pos = pos.Value;
            }

            return pos;
        }
    }

    private (double X, double Y) _lastRow03Pos = (0, 0);
    public (double X, double Y)? FishPositionRow03
    {
        get
        {
            var pos = GetFishPosFromRow(SpearfishingRow.Row03);
            if (pos != null)
            {
                _lastRow03Pos = pos.Value;
            }

            return pos;
        }
    }

    public double? FishPositionRow01XDelta => 
        GetFishPosXDeltaFromRow(SpearfishingRow.Row01);

    public double? FishPositionRow02XDelta => 
        GetFishPosXDeltaFromRow(SpearfishingRow.Row02);

    public double? FishPositionRow03XDelta => 
        GetFishPosXDeltaFromRow(SpearfishingRow.Row03);

    public FishSize? FishSizeRow01 =>
        GetFishSizeFromRow(SpearfishingRow.Row01);
    
    public FishSize? FishSizeRow02 =>
        GetFishSizeFromRow(SpearfishingRow.Row02);
    
    public FishSize? FishSizeRow03 =>
        GetFishSizeFromRow(SpearfishingRow.Row03);
}
