namespace SizeMattersFishingLib.Spearfishing;

public interface IFishPositions
{
    public (double X, double Y)? FishPositionRow01 { get; }
    public (double X, double Y)? FishPositionRow02 { get; }
    public (double X, double Y)? FishPositionRow03 { get; }
    public double? FishPositionXDeltaRow01 { get; }
    public double? FishPositionXDeltaRow02 { get; }
    public double? FishPositionXDeltaRow03 { get; }
    public FishSize? FishSizeRow01 { get; }
    public FishSize? FishSizeRow02 { get; }
    public FishSize? FishSizeRow03 { get; }
}
