using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SendJSON.Windows;
using Dalamud.Game.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace SendJSON;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/sendjson";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SendJSON");
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        // You might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle the config window. Use \"/jsonsend command arg1 arg2...\" to send a configured command with args replacing [#NUMBER#] style placeholders."
        });

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // Config is main and settings window.
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUi;

    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    HttpClient httpClient = new HttpClient();



    private void OnCommand(string command, string args)
    {

        if (args == "")
        {
            ConfigWindow.Toggle();
            return;
        }

        Regex rg = new Regex("\\[#(\\d+)#\\]");
        var argsSplit = args.Split(" ").ToList();
        var subCommand = argsSplit[0];
        argsSplit.RemoveAt(0);
        for (int i = 0; i < Configuration.commandList.Count; i++)
        {
            if (Configuration.commandList[i].CommandName == subCommand)
            {
                string formatString = Configuration.commandList[i].CommandValue;
                string requestString = rg.Replace(formatString, match =>
                {
                    if (match.Groups[0].Success)
                    {
                        int index = Int32.Parse(match.Groups[1].Value);
                        if (index < argsSplit.Count)
                        {
                            return argsSplit[index];
                        }
                    }
                    return match.Value;
                });
                Task.Run(async () =>
                {
                    try
                    {
                        Log.Information($"Sending: {requestString}");
                        var sc = new StringContent(requestString, UnicodeEncoding.UTF8, "application/json");
                        var res = await httpClient.PostAsync(
                            $"{Configuration.url}", sc);
                        Log.Information($"Response: {res}");
                    }
                    catch (Exception e)
                    {
                        Log.Information($"{e}");
                    }
                });
            }
        }
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
}
