using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using SizeMattersFishing.GUI;
using SizeMattersFishing.Spearfishing;
using System.Reflection;
using System;


namespace SizeMattersFishing;

public class Plugin : IDalamudPlugin
{
    public string Name => "Size Matters when Fishing";
    public string AssemblyLocation { get; set; } = Assembly.GetExecutingAssembly().Location;
    public DalamudPluginInterface PluginInterface { get; }
    public GameGui GameGui { get; }
    public WindowSystem WindowSystem { get; }
    public Config PluginConfig { get; }
    public SpearfishingData SpearfishingData { get; }
    public DalamudOverlayContainer OverlayGuiContainer { get; }

    public Plugin
    (
        DalamudPluginInterface pluginInterface,
        //BuddyList buddies,
        //ChatGui chat,
        //ChatHandlers chatHandlers,
        //ClientState clientState,
        //CommandManager commands,
        //Condition condition,
        //DataManager data,
        //FateTable fates,
        //FlyTextGui flyText,
        //Framework framework,
        GameGui gameGui
        //GameNetwork gameNetwork,
        //JobGauges gauges,
        //KeyState keyState,
        //LibcFunction libcFunction,
        //ObjectTable objects,
        //PartyFinderGui pfGui,
        //PartyList party,
        //SeStringManager seStringManager,
        //SigScanner sigScanner
        //TargetManager targets,
        //ToastGui toasts
    )
    {
        PluginInterface = pluginInterface;
        GameGui = gameGui;

        var assemblyVersion = (AssemblyInformationalVersionAttribute) Assembly
            .GetExecutingAssembly()
            .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)[0];

        PluginConfig = (Config) (pluginInterface.GetPluginConfig() ?? new Config());
        PluginConfig.Init(this);
        PluginConfig.LastPluginVersion = assemblyVersion.InformationalVersion;

        SpearfishingData = new SpearfishingData(GameGui);
        OverlayGuiContainer = new DalamudOverlayContainer(SpearfishingData);
        WindowSystem = new WindowSystem(Name);
        OverlayGuiContainer.RegisterWindows(WindowSystem);

        PluginInterface.UiBuilder.Draw += BuildUi;
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= BuildUi;
        GC.SuppressFinalize(this);
    }

    private void BuildUi()
    {
        var addon = GameGui.GetAddonByName("SpearFishing", 1);
        if (addon != IntPtr.Zero)
        {
            OverlayGuiContainer.IsOpen = true;
            OverlayGuiContainer.PrepareForRender(addon);
        }
        else
        {
            OverlayGuiContainer.IsOpen = false;
        }

        WindowSystem.Draw();
    }
}
