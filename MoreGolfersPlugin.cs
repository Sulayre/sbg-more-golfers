using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx.Configuration;

namespace MoreGolfers;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class MoreGolfersPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public static ConfigEntry<float> MaxPlayersConfig;
    
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
        return MoreGolfersPlugin.MaxPlayersConfig.Value;
    }
}

[HarmonyPatch(typeof(MatchSetupMenu), "<LoadValues>b__83_0")]
public static class SliderLogicPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        bool found = false;
        int finds = 0;
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].OperandIs(16))
            {
                codes[i].operand = (int) MoreGolfersPlugin.GetCustomMaxPlayers() ;
                finds++;
                found = true;
                if (finds > 1)
                {
                    MoreGolfersPlugin.Logger.LogInfo($"[{MyPluginInfo.PLUGIN_GUID}]: patched player limit from 16 to 32.");
                    break;   
                }
            }
        }

        if (!found)
            MoreGolfersPlugin.Logger.LogWarning($"[{MyPluginInfo.PLUGIN_GUID}]: instruction Ldc_I4_S was not found with value 16.");

        return codes.AsEnumerable();
    }
}

[HarmonyPatch(typeof(MatchSetupMenu), "OnStartClient")]
public static class MatchSetupMenu_Patch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool found = false;
        foreach (var instruction in instructions)
        {
            if (!found && instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 16f)
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = AccessTools.Method(typeof(MatchSetupMenu_Patch), nameof(MoreGolfersPlugin.GetCustomMaxPlayers));
                found = true;
                MoreGolfersPlugin.Logger.LogInfo(
                    $"[{MyPluginInfo.PLUGIN_GUID}]: Patched onStartClient to handle increased players on room configuration.");
            }

            yield return instruction;
        }
    }
}