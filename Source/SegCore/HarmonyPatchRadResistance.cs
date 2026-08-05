using Verse;
using RimWorld;
using UnityEngine;

namespace seg
{
    public class HediffCompProperties_RadResistanceScale : HediffCompProperties
    {
        public HediffCompProperties_RadResistanceScale()
        {
            this.compClass = typeof(HediffComp_RadResistanceScale);
        }
    }

    public class HediffComp_RadResistanceScale : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            Pawn pawn = this.Pawn;
            if (pawn == null)
                return;
            StatDef radResStat = StatDef.Named("Seg_RadResistance");
            float resistance = pawn.GetStatValue(radResStat);
            resistance = Mathf.Clamp01(resistance);
            float multiplier = 1f - resistance;

            Hediff hediff = this.parent;
            hediff.Severity *= multiplier;

            if (hediff.Severity <= 0f)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
    }
}