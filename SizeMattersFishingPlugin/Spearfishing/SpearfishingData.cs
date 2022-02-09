using Dalamud.Game.Gui;
using SizeMattersFishingLib.Spearfishing;


namespace SizeMattersFishing.Spearfishing;

public class SpearfishingData : ISpearfishingData
{
    public IFishPositions FishPositions { get; }

    public SpearfishingData(GameGui gameGui)
    {
        FishPositions = new FishPositions(gameGui);
    }
}
