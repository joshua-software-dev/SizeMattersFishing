using Dalamud.Configuration;
using JetBrains.Annotations;
using SizeMattersFishing.State;
using System;


namespace SizeMattersFishing;

public class Config : IPluginConfiguration, IConfigWrapper
{
    public string LastPluginVersion { get; set; } = null!;
    public int Version { get; set; }

    [PublicAPI]
    [NonSerialized]
    public Plugin Plugin = null!;

    public void Init(Plugin plugin)
    {
        Plugin = plugin;
    }

    public void Save() =>
        Plugin.PluginInterface.SavePluginConfig(this);
}
