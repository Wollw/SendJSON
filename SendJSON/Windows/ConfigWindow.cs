using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.IoC;

namespace SendJSON.Windows;

public class ConfigWindow : Window, IDisposable
{
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    private readonly Configuration configuration;

    // We give this window a constant ID using ###.
    // This allows for labels to be dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("SendJSON Config")
    {
        //Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        //        ImGuiWindowFlags.NoScrollWithMouse;

        //Size = new Vector2(1000, 500);
        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }




    public override void Draw()
    {

        // Can't ref a property, so use a local copy
        var url = configuration.url;

        if (ImGui.Button("Save"))
        {
            configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.Button("Add"))
        {
            configuration.length++;
            configuration.commandList.Add(new CommandEntry("", ""));
        }
        if (ImGui.InputText("Server", ref url))
        {
            configuration.url = url;
            // Can save immediately on change if you don't want to provide a "Save and Close" button
        }

        var toRemove = new List<int>();
        for (int i = 0; i < configuration.length; i++)
        {
            try
            {
                var name = configuration.commandList[i].CommandName;
                var value = configuration.commandList[i].CommandValue;
                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.PushID($"Command {i}");
                ImGui.InputText($"Command", ref name);
                ImGui.PopID();
                ImGui.PushID($"JSON {i}");
                ImGui.InputText($"JSON", ref value);
                ImGui.PopID();
                ImGui.PushID($"Remove {i}");
                if (ImGui.Button("—"))
                {
                    toRemove.Add(i);
                }
                else
                {
                    configuration.commandList[i] = new CommandEntry(name, value);
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("Remove");
            }
            catch
            {
            }
        }
        if (toRemove.Count > 0)
            for (var i = 0; i < toRemove.Count; i++)
            {
                configuration.commandList.RemoveAt(toRemove[i]);
            }


    }
}
