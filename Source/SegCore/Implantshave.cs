using RimWorld;
using Verse;
using Core40k;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace seg;

public class HediffCompProperties_ImplantShave : HediffCompProperties
{
    public HediffCompProperties_ImplantShave()
    {
        compClass = typeof(HediffComp_ImplantShave);
    }
}

public class HediffComp_ImplantShave : HediffComp
{
    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);

        var pawn = Pawn;
        if (pawn == null || pawn.Dead)
        {
            return;
        }

        pawn.story?.hairDef = HairDefOf.Bald;


        if (ModsConfig.IdeologyActive && pawn.style != null)
        {
            pawn.style.beardDef = BeardDefOf.NoBeard;
        }
    }
}
    
[HarmonyPatch(typeof(DynamicPawnRenderNodeSetup_DecorativeAddons), "GetDynamicNodes")]
public static class Patch_DecorativeAddons_GetDynamicNodes
{
    [HarmonyPrefix]
    public static bool Prefix(Pawn pawn, PawnRenderTree tree, ref IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> __result)
    {
        if (pawn == null)
        {
            __result = Enumerable.Empty<(PawnRenderNode, PawnRenderNode)>();
            return false;
        }

        if (pawn.apparel == null || pawn.apparel.WornApparelCount == 0)
            return true;

        if (pawn.story == null)
        {
            __result = Enumerable.Empty<(PawnRenderNode, PawnRenderNode)>();
            return false;
        }

        foreach (var apparel in pawn.apparel.WornApparel)
        {
            var decorativeComp = apparel.GetComp<CompDecorative>();
            if (decorativeComp == null)
            {
                continue;
            }

            if (decorativeComp == null)
            {
                __result = Enumerable.Empty<(PawnRenderNode, PawnRenderNode)>();
                return false;
            }
        }

        return true;
    }
}