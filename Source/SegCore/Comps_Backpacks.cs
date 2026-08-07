using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using Core40k;

namespace seg
{
    public class CompHasBackpack : ThingComp
    {
    }

    public class CompProperties_HasBackpack : CompProperties
    {
        public CompProperties_HasBackpack()
        {
            this.compClass = typeof(CompHasBackpack);
        }
    }

    public class CompHideWithBackpack : ThingComp
    {
    }

    public class CompProperties_HideWithBackpack : CompProperties
    {
        public CompProperties_HideWithBackpack()
        {
            this.compClass = typeof(CompHideWithBackpack);
        }
    }

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class HideAccessoryWithBackpack_Postfix
    {
        public static void Postfix(
            Apparel apparel,
            BodyTypeDef bodyType,
            bool forStatue,
            ref bool __result,
            ref ApparelGraphicRecord rec)
        {
            if (!__result)
                return;

            if (!apparel.HasComp<CompHideWithBackpack>())
                return;

            Pawn pawn = apparel.Wearer;
            if (pawn == null || pawn.apparel == null)
                return;

            bool hasBackpack = pawn.apparel.WornApparel.Any(a => a.HasComp<CompHasBackpack>());
            if (!hasBackpack)
                return;

            rec = new ApparelGraphicRecord(null, null);
            __result = false;
        }
    }
}