using SizeMattersFishingLib.Spearfishing;


namespace PluginUiTestbed.MockImplementations;

public class MockFishPositions : IFishPositions
{
    public (double X, double Y)? FishPositionRow01 { get; } = (1, 1);
    public (double X, double Y)? FishPositionRow02 { get; } = (2, 2);
    public (double X, double Y)? FishPositionRow03 { get; } = (3, 3);
    public double? FishPositionXDeltaRow01 { get; } = 1;
    public double? FishPositionXDeltaRow02 { get; } = 1;
    public double? FishPositionXDeltaRow03 { get; } = 1;
    public FishSize? FishSizeRow01 { get; } = FishSize.Small;
    public FishSize? FishSizeRow02 { get; } = FishSize.Medium;
    public FishSize? FishSizeRow03 { get; } = FishSize.Large;
}