namespace SizeMattersFishingLib.Spearfishing;

public interface IFishPositions
{
    public (double X, double Y)? FishPositionRow01 { get; }
    public (double X, double Y)? FishPositionRow02 { get; }
    public (double X, double Y)? FishPositionRow03 { get; }
    public double? FishPositionRow01XDelta { get; }
    public double? FishPositionRow02XDelta { get; }
    public double? FishPositionRow03XDelta { get; }
    public FishSize? FishSizeRow01 { get; }
    public FishSize? FishSizeRow02 { get; }
    public FishSize? FishSizeRow03 { get; }
}
