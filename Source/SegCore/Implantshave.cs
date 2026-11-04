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
    