using VEF.Abilities;
using Verse;

namespace Seg
{
    public class HediffComp_GivesAbility : HediffComp
    {
        private HediffCompProperties_GivesAbility Props => (HediffCompProperties_GivesAbility)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            // VANILLA
            if (Props.vanillaAbility != null)
            {
                Pawn.abilities?.GainAbility(Props.vanillaAbility);
            }

            // VEF
            if (Props.vefAbility != null)
            {
                var comp = Pawn.GetComp<CompAbilities>();
                comp?.GiveAbility(Props.vefAbility);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            // VANILLA
            if (Props.vanillaAbility != null)
            {
                Pawn.abilities?.RemoveAbility(Props.vanillaAbility);
            }

            // VEF
            if (Props.vefAbility != null)
            {
                var comp = Pawn.GetComp<CompAbilities>();
                comp?.LearnedAbilities.RemoveAll(a => a.def == Props.vefAbility);
            }
        }
    }

    public class HediffCompProperties_GivesAbility : HediffCompProperties
    {
        public RimWorld.AbilityDef vanillaAbility;
        public AbilityDef vefAbility;

        public HediffCompProperties_GivesAbility()
        {
            compClass = typeof(HediffComp_GivesAbility);
        }
    }
}
