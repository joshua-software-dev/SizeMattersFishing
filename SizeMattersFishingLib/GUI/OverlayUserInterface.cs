using ImGuiNET;
using SizeMattersFishingLib.Spearfishing;
using System.Globalization;
using System;


namespace SizeMattersFishingLib.GUI;

public class OverlayUserInterface : IUserInterface
{
    private readonly ISpearfishingData _spearfishingData;
    private readonly SpearfishingRow _row;

    public OverlayUserInterface(ISpearfishingData spearData, SpearfishingRow row) => 
        (_spearfishingData, _row) = (spearData, row);

    public void Draw()
    {
        var deltaForRow = _row switch
        {
            SpearfishingRow.Row01 => _spearfishingData.FishPositions.FishPositionXDeltaRow01,
            SpearfishingRow.Row02 => _spearfishingData.FishPositions.FishPositionXDeltaRow02,
            SpearfishingRow.Row03 => _spearfishingData.FishPositions.FishPositionXDeltaRow03,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (deltaForRow == null) return;

        var drawValue = deltaForRow.Value.ToString("F3", CultureInfo.InvariantCulture);
        if (deltaForRow != 0)
        {
            var size = _row switch
            {
                SpearfishingRow.Row01 => _spearfishingData.FishPositions.FishSizeRow01,
                SpearfishingRow.Row02 => _spearfishingData.FishPositions.FishSizeRow02,
                SpearfishingRow.Row03 => _spearfishingData.FishPositions.FishSizeRow03,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            drawValue += size switch
            {
                FishSize.Bottle => "\nB",
                FishSize.Small => "\nS",
                FishSize.Medium => "\nM",
                FishSize.Large => "\nL",
                _ => ""
            };
        }

        ImGui.Text(drawValue);
    }
}
