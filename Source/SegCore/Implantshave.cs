using RimWorld;
using Verse;

namespace seg
{
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

            Pawn pawn = Pawn;
            if (pawn == null || pawn.Dead) return;

            // Shave hair once
            pawn.story.hairDef = HairDefOf.Bald;
            // Shave beard once
            pawn.style.beardDef = BeardDefOf.NoBeard;
        }
    }
}