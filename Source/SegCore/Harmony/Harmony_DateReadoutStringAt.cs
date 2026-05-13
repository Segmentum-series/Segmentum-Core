using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Seg.Harmony;

public class Harmony_DateReadoutStringAt
{
    private static SegCoreModSettings modSettings = null;
    public static SegCoreModSettings ModSettings => modSettings ??= LoadedModManager.GetMod<SegCoreMod>().GetSettings<SegCoreModSettings>();
    
    [HarmonyPatch(typeof (GenDate), "DateReadoutStringAt")]
    public static class Patch_DateReadoutStringAt
    {
        private static void Postfix(ref string __result, long absTicks, Vector2 location)
        {
            if (!ModSettings.useImperialFormat)
            {
                return;
            }
                
            var vanillaYear = GenDate.Year(absTicks, location.x);
            var newValue = SegCoreUtils.FormatImperialYear(vanillaYear);
            
            __result = __result.Replace(vanillaYear.ToString(), newValue);
        }
    }
}

//Below unused?
/*public static class Patch_DateFullStringAt
{
    private static void Postfix(ref string __result, long absTicks, Vector2 location)
    {
        if (!ImperialDateMod.Settings.useImperialFormat)
            return;
        string str = ImperialDateUtility.FormatImperialYear(GenDate.Year(absTicks, location.x));
        int num = __result.LastIndexOf(',');
        if (num < 0)
            return;
        __result = $"{__result.Substring(0, num + 1)} {str}";
    }
}*/