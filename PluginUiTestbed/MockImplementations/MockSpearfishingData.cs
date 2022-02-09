using SizeMattersFishingLib.Spearfishing;


namespace PluginUiTestbed.MockImplementations;

public class MockSpearfishingData : ISpearfishingData
{
    public IFishPositions FishPositions { get; }

    public MockSpearfishingData()
    {
        FishPositions = new MockFishPositions();
    }
}