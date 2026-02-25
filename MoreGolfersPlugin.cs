using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using System.Reflection;

namespace MoreGolfers;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class MoreGolfersPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public static ConfigEntry<float> MaxPlayersConfig;
    private static Type _gmType = typeof(GameManager);
    private void Awake()
    {
        Logger = base.Logger;
        MaxPlayersConfig = Config.Bind("General", "MaxPlayers", 32f, "Player limit");
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded and patches applied!");
    }
    public static float GetCustomMaxPlayers()
    {
        return MaxPlayersConfig.Value;
    }
    public static float GetCurrentPlayerCount()
    {
        Logger.LogInfo("Attempting to get current player count");
        var gmInstance = FindFirstObjectByType(_gmType);
        if (gmInstance != null)
        {
            try
            {
                var field = _gmType.GetField("remotePlayers",
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);
                if (field != null)
                {
                    var listValue = field.GetValue(gmInstance);
                    if (listValue is System.Collections.IList list)
                    {
                        float count = list.Count + 1;
                        Logger.LogInfo($"{count} players found");
                        return count;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Error accessing remotePlayers: {e.Message}");
            }
        }
        Logger.LogWarning("GameManager has not been initialized");
        return GetCustomMaxPlayers();
    }
}
