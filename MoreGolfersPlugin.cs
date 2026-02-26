using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using UnityEngine;

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
    
    public static int GetCurrentPlayerCount()
    {
        var connectionIds = BNetworkManager.ServerConnectionIds;
        return (connectionIds != null) ? connectionIds.Count : 1;
    }
    
    public static int GetActivePlatformCount()
    {
        return GetCurrentPlayerCount() > 8 ? 4 : 2;
    }
    
    public static int GetCurrentPlayersPerPlatform()
    {
        int total = (int)GetCurrentPlayerCount();
        int platforms = GetActivePlatformCount();
        return Mathf.Max(1, Mathf.CeilToInt((float)total / platforms));
    }

    public static float GetDistanceBetweenTees()
    {
        float vanillaDistance = 3.25f;
        int playersPerPlat = GetCurrentPlayersPerPlatform();
        if (GetCurrentPlayerCount() <= 16 || playersPerPlat <= 1)
        {
            return vanillaDistance;
        }
        float moddedDistance = 12f / (playersPerPlat - 1);
        return Mathf.Min(vanillaDistance, moddedDistance);
    }
    
    public static float GetFirstTeeOffset()
    {
        int count = GetCurrentPlayersPerPlatform();
        if (count <= 1) return 0f;
        float totalWidth = (count - 1) * GetDistanceBetweenTees();
        return totalWidth / 2f;
    }
}
