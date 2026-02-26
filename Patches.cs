using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace MoreGolfers;

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
                int newValue = (int)MoreGolfersPlugin.GetCustomMaxPlayers();
                codes[i] = new CodeInstruction(OpCodes.Ldc_I4, newValue);
                finds++;
                found = true;

                if (finds > 1)
                {
                    //MoreGolfersPlugin.Logger.LogInfo("Patched MatchSetupMenu delegate");
                    break;
                }
            }
        }

        if (!found)
            MoreGolfersPlugin.Logger.LogWarning("instruction Ldc_I4_S was not found with value 16.");

        return codes.AsEnumerable();
    }
}

[HarmonyPatch(typeof(MatchSetupMenu), "OnStartClient")]
public static class PatchMatchSetupMenu
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool found = false;
        foreach (var instruction in instructions)
        {
            if (!found && instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == 16f)
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = AccessTools.Method(typeof(MoreGolfersPlugin), nameof(MoreGolfersPlugin.GetCustomMaxPlayers));
                found = true;
                //MoreGolfersPlugin.Logger.LogInfo("Patched MatchSetupMenu.onStartClient");
            }

            yield return instruction;
        }
    }
}

[HarmonyPatch(typeof(TeeingPlatformSettings), "MaxTeeCount", MethodType.Getter)]
class PatchMaxTeeCount
{
    static bool Prefix(ref int __result)
    {
        __result = MoreGolfersPlugin.GetCurrentPlayersPerPlatform();
        return false;
    }
}

[HarmonyPatch(typeof(TeeingPlatformSettings), "DistanceBetweenTees", MethodType.Getter)]
class PatchDistanceBetweenTees
{
    static bool Prefix(ref float __result)
    {
        __result = MoreGolfersPlugin.GetDistanceBetweenTees();
        return false;
    }
}

[HarmonyPatch(typeof(TeeingPlatformSettings), "FirstTeeOffset", MethodType.Getter)]
class PatchFirstTeeOffset
{
    static bool Prefix(ref float __result)
    {
        __result = MoreGolfersPlugin.GetFirstTeeOffset();
        return false;
    }
}

/*
[HarmonyPatch(typeof(PlayerOcclusionManager), "Awake")]
public static class PatchPlayerOcclusionManager
{
    static bool Prefix(PlayerOcclusionManager __instance)
    {
        __instance.EnsureSingleton();
        __instance.transforms = new TransformAccessArray((int)MoreGolfersPlugin.GetCustomMaxPlayers());
        __instance.visibilty = new NativeList<PlayerOcclusionManager.State>((int)MoreGolfersPlugin.GetCustomMaxPlayers()/2, Allocator.Persistent);
        //MoreGolfersPlugin.Logger.LogInfo("Patched PlayerOcclusionManager.Awake");
        return false;
    }

}
*/