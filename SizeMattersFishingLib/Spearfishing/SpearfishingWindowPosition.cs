namespace SizeMattersFishingLib.Spearfishing;

public readonly struct SpearfishingWindowPosition
{
    public readonly float PosX;
    public readonly float PosY;
    public readonly float ScaleX;
    public readonly float ScaleY;

    public SpearfishingWindowPosition(float posX, float posY, float scaleX, float scaleY) =>
        (PosX, PosY, ScaleX, ScaleY) = (posX, posY, scaleX, scaleY);
}