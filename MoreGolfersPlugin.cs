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
        //Logger.LogInfo("Attempting to get current player count");
        var connectionIds = BNetworkManager.ServerConnectionIds;
        if (connectionIds != null)
        {
            var count = connectionIds.Count;
            //Logger.LogInfo($"{count} players found");
            return count;
        }
        Logger.LogWarning("BNetworkManager's singleton has not been initialized");
        return GetCustomMaxPlayers();
    }
}
