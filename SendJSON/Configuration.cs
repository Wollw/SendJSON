using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SendJSON;

[Serializable]
public struct CommandEntry
{
    public String CommandName { get; set; }
    public String CommandValue { get; set; }

    public CommandEntry(String name, String value)
    {
        this.CommandName = name;
        this.CommandValue = value;
    }

}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public List<CommandEntry> commandList = new List<CommandEntry>();
    public int length = 0;

    public string url { get; set; } = "http://localhost:1234";

    // The below exists just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
