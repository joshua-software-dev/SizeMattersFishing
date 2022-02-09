using Dalamud.Game.Gui;
using SizeMattersFishing.Structs;
using SizeMattersFishingLib.Spearfishing;
using System;


namespace SizeMattersFishing.Spearfishing;

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

            if 
            (
                spearfishingAddon->AtkUnitBase.UldManager.NodeList != null && 
                spearfishingAddon->AtkUnitBase.UldManager.NodeListCount >= position
            )
            {
                var fishContainer = spearfishingAddon->AtkUnitBase.UldManager.NodeList[position];
                if (fishContainer != null)
                {
                    return (fishContainer->X, fishContainer->Y);
                }
            }

            return null;
        }
    }

    private double? GetFishPosXDeltaFromRow(SpearfishingRow row)
    {
        var previousX = row switch
        {
            SpearfishingRow.Row01 => _lastPositionRow01.X,
            SpearfishingRow.Row02 => _lastPositionRow02.X,
            SpearfishingRow.Row03 => _lastPositionRow03.X,
            _ => throw new ArgumentOutOfRangeException(nameof(row), row, null)
        };

        var currentPos = GetFishPosFromRow(row);
        if (currentPos == null) return null;

        switch (row)
        {
            case SpearfishingRow.Row01:
            {
                _lastPositionRow01 = currentPos.Value;
                break;
            }
            case SpearfishingRow.Row02:
            {
                _lastPositionRow02 = currentPos.Value;
                break;
            }
            case SpearfishingRow.Row03:
            {
                _lastPositionRow03 = currentPos.Value;
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(row), row, null);
            }
        }

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

            if 
            (
                spearfishingAddon->AtkUnitBase.UldManager.NodeList == null ||
                spearfishingAddon->AtkUnitBase.UldManager.NodeList[position] == null ||
                !spearfishingAddon->AtkUnitBase.UldManager.NodeList[position]->IsVisible
            )
            {
                return null;
            }

            var fishContainer = spearfishingAddon->AtkUnitBase.UldManager.NodeList[position];
            var fishContainerNodeList = fishContainer->GetAsAtkComponentNode()->Component->UldManager.NodeList;
            if (fishContainerNodeList == null) return null;

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

    private (double X, double Y) _lastPositionRow01 = (0, 0);
    public (double X, double Y)? FishPositionRow01
    {
        get
        {
            var pos = GetFishPosFromRow(SpearfishingRow.Row01);
            if (pos != null)
            {
                _lastPositionRow01 = pos.Value;
            }

            return pos;
        }
    }

    private (double X, double Y) _lastPositionRow02 = (0, 0);
    public (double X, double Y)? FishPositionRow02
    {
        get
        {
            var pos = GetFishPosFromRow(SpearfishingRow.Row02);
            if (pos != null)
            {
                _lastPositionRow02 = pos.Value;
            }

            return pos;
        }
    }

    private (double X, double Y) _lastPositionRow03 = (0, 0);
    public (double X, double Y)? FishPositionRow03
    {
        get
        {
            var pos = GetFishPosFromRow(SpearfishingRow.Row03);
            if (pos != null)
            {
                _lastPositionRow03 = pos.Value;
            }

            return pos;
        }
    }

    public double? FishPositionXDeltaRow01 => 
        GetFishPosXDeltaFromRow(SpearfishingRow.Row01);

    public double? FishPositionXDeltaRow02 => 
        GetFishPosXDeltaFromRow(SpearfishingRow.Row02);

    public double? FishPositionXDeltaRow03 => 
        GetFishPosXDeltaFromRow(SpearfishingRow.Row03);

    public FishSize? FishSizeRow01 =>
        GetFishSizeFromRow(SpearfishingRow.Row01);
    
    public FishSize? FishSizeRow02 =>
        GetFishSizeFromRow(SpearfishingRow.Row02);
    
    public FishSize? FishSizeRow03 =>
        GetFishSizeFromRow(SpearfishingRow.Row03);
}
