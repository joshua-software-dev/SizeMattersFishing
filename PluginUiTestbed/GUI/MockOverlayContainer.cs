using PluginUiTestbed.MockImplementations;
using SizeMattersFishingLib.GUI;
using SizeMattersFishingLib.Spearfishing;
using System.Globalization;
using System.Numerics;


namespace PluginUiTestbed.GUI;

public class MockOverlayContainer : IUserInterface
{
    private string Row01Value
    {
        get => _row01Window.DrawValue; 
        set => _row01Window.DrawValue = value;
    }

    private string Row02Value
    {
        get => _row02Window.DrawValue; 
        set => _row02Window.DrawValue = value;
    }

    private string Row03Value
    {
        get => _row03Window.DrawValue; 
        set => _row03Window.DrawValue = value;
    }

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
    private readonly ISpearfishingData _spearfishingData;

    public MockOverlayContainer()
    {
        _spearfishingData = new MockSpearfishingData();
        _row01Window = new MockOverlayWindow("SpearFishingRow01");
        _row02Window = new MockOverlayWindow("SpearFishingRow02");
        _row03Window = new MockOverlayWindow("SpearFishingRow03");
    }

    public void PrepareForRender()
    {
        var deltaRow01 = _spearfishingData.FishPositions.FishPositionRow01XDelta!;
        Row01Value = deltaRow01.Value.ToString("F3", CultureInfo.InvariantCulture);
        if (deltaRow01 != 0)
        {
            Row01Value += _spearfishingData.FishPositions.FishSizeRow01 switch
            {
                FishSize.Bottle => "\nB",
                FishSize.Small => "\nS",
                FishSize.Medium => "\nM",
                FishSize.Large => "\nL",
                _ => ""
            };
        }

        var deltaRow02 = _spearfishingData.FishPositions.FishPositionRow02XDelta!;
        Row02Value = deltaRow02.Value.ToString("F3", CultureInfo.InvariantCulture);
        if (deltaRow02 != 0)
        {
            Row02Value += _spearfishingData.FishPositions.FishSizeRow02 switch
            {
                FishSize.Bottle => "\nB",
                FishSize.Small => "\nS",
                FishSize.Medium => "\nM",
                FishSize.Large => "\nL",
                _ => ""
            };
        }

        var deltaRow03 = _spearfishingData.FishPositions.FishPositionRow03XDelta!;
        Row03Value = deltaRow03.Value.ToString("F3", CultureInfo.InvariantCulture);
        if (deltaRow03 != 0)
        {
            Row03Value += _spearfishingData.FishPositions.FishSizeRow03 switch
            {
                FishSize.Bottle => "\nB",
                FishSize.Small => "\nS",
                FishSize.Medium => "\nM",
                FishSize.Large => "\nL",
                _ => ""
            };
        }

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
