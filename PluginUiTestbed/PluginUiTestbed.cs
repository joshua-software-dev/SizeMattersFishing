using PluginUiTestbed.GUI;
using System.Linq;
using System.Reflection;
using System;
using Veldrid;


namespace PluginUiTestbed;

public static class PluginUiTestbed
{
    private static GraphicsBackend? SelectGraphicsBackend(string? preference) =>
        preference switch
        {
            "DIRECTX" => GraphicsBackend.Direct3D11,
            "METAL" => GraphicsBackend.Metal,
            "OPENGL" => GraphicsBackend.OpenGL,
            "OPENGLES" => GraphicsBackend.OpenGLES,
            "VULKAN" => GraphicsBackend.Vulkan,
            null => null,
            _ => throw new ArgumentException(message: "Invalid graphics backend")
        };

    public static void Main(string[] args)
    {
        var backend = new UiBackend(
            backendProvider: SelectGraphicsBackend(args.FirstOrDefault()?.ToUpperInvariant()),
            windowX: 800,
            windowY: 800
        );

        var assemblyVersion = (AssemblyInformationalVersionAttribute) Assembly
            .GetExecutingAssembly()
            .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0];

        var container = new MockOverlayContainer();
        container.IsOpen = true;

        backend.DrawLoop(container);
    }
}
