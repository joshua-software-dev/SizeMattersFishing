using SizeMattersFishingLib.GUI;
using System.Numerics;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid;


namespace PluginUiTestbed;

public class UiBackend
{
    private readonly CommandList _commandList;
    private readonly ImGuiController _controller;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Sdl2Window _window;

    private static readonly Vector3 BackgroundColor = new (0.45f, 0.55f, 0.6f);

    public UiBackend(GraphicsBackend? backendProvider, int windowX, int windowY)
    {
        var windowCreateInfo = new WindowCreateInfo(
            x: 50,
            y: 50,
            windowWidth: windowX,
            windowHeight: windowY,
            windowInitialState: WindowState.Normal,
            windowTitle: "Dalamud ImGui Testbed - Backend: "
        );

        var graphicsDeviceOptions = new GraphicsDeviceOptions(
            debug: true,
            swapchainDepthFormat: null,
            syncToVerticalBlank: true,
            resourceBindingModel: ResourceBindingModel.Improved,
            preferDepthRangeZeroToOne: true,
            preferStandardClipSpaceYDirection: true
        );

        if (backendProvider != null)
        {
            windowCreateInfo.WindowTitle += backendProvider.Value;
            VeldridStartup.CreateWindowAndGraphicsDevice(
                windowCI: windowCreateInfo,
                deviceOptions: graphicsDeviceOptions,
                preferredBackend: backendProvider.Value,
                window: out _window,
                gd: out _graphicsDevice
            );
        }
        else
        {
            VeldridStartup.CreateWindowAndGraphicsDevice(
                windowCI: windowCreateInfo,
                deviceOptions: graphicsDeviceOptions,
                window: out _window,
                gd: out _graphicsDevice
            );

            _window.Title = windowCreateInfo.WindowTitle + "Default(" + _graphicsDevice.BackendType + ")";
        }

        _commandList = _graphicsDevice.ResourceFactory.CreateCommandList();

        _controller = new ImGuiController(
            _graphicsDevice,
            _graphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
            _window.Width,
            _window.Height
        );

        _window.Resized += () =>
        {
            _graphicsDevice.MainSwapchain.Resize((uint) _window.Width, (uint) _window.Height);
            _controller.WindowResized(_window.Width, _window.Height);
        };
    }

    public void DrawLoop(IUserInterface drawable)
    {
        // Main application loop
        while (_window.Exists)
        {
            var snapshot = _window.PumpEvents();
            if (!_window.Exists)
            {
                break;
            }

            // Feed the input events to ImGui controller, which passes them through to ImGui.
            _controller.Update(1f / 60f, snapshot);

            drawable.Draw();

            _commandList.Begin();
            _commandList.SetFramebuffer(_graphicsDevice.MainSwapchain.Framebuffer);

            _commandList.ClearColorTarget(
                0,
                new RgbaFloat(
                    BackgroundColor.X,
                    BackgroundColor.Y,
                    BackgroundColor.Z,
                    1f
                )
            );

            _controller.Render(_graphicsDevice, _commandList);
            _commandList.End();
            _graphicsDevice.SubmitCommands(_commandList);
            _graphicsDevice.SwapBuffers(_graphicsDevice.MainSwapchain);
        }

        // Clean up Veldrid resources
        _graphicsDevice.WaitForIdle();
        _controller.Dispose();
        _commandList.Dispose();
        // _graphicsDevice.Dispose();
    }
}
