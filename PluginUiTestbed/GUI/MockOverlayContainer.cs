using PluginUiTestbed.MockImplementations;
using SizeMattersFishingLib.GUI;
using SizeMattersFishingLib.Spearfishing;
using System.Globalization;
using System.Numerics;


namespace PluginUiTestbed.GUI;

public class MockOverlayContainer : IUserInterface
{
    public bool IsOpen
    {
        set
        {
            _row01Window.IsOpen = value;
            _row02Window.IsOpen = value;
            _row03Window.IsOpen = value;
        }
    }
    
    private readonly MockOverlayWindow _row01Window;
    private readonly MockOverlayWindow _row02Window;
    private readonly MockOverlayWindow _row03Window;

    public MockOverlayContainer()
    {
        var spearfishingData = new MockSpearfishingData();
        _row01Window = new MockOverlayWindow("SpearFishingRow01", spearfishingData, SpearfishingRow.Row01);
        _row02Window = new MockOverlayWindow("SpearFishingRow02", spearfishingData, SpearfishingRow.Row02);
        _row03Window = new MockOverlayWindow("SpearFishingRow03", spearfishingData, SpearfishingRow.Row03);
    }

    public void PrepareForRender()
    {
        _row01Window.Position = new Vector2(50, 155f * 1.2f);
        _row02Window.Position = new Vector2(50, 230f * 1.2f);
        _row03Window.Position = new Vector2(50, 305f * 1.2f);
    }

    public void Draw()
    {
        PrepareForRender();

        _row01Window.PreDraw();
        _row01Window.Draw();
        _row01Window.PostDraw();
        
        _row02Window.PreDraw();
        _row02Window.Draw();
        _row02Window.PostDraw();
        
        _row03Window.PreDraw();
        _row03Window.Draw();
        _row03Window.PostDraw();
    }
}
