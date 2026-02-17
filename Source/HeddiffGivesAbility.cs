using RimWorld;
using Verse;
using System.Reflection;
using System.Collections.Generic;
using VEF.Abilities;

namespace Seg
{
    public class HediffComp_GivesAbility : HediffComp
    {
        public HediffCompProperties_GivesAbility Props => (HediffCompProperties_GivesAbility)props;

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
                if (comp == null)
                    return;

                var field = typeof(CompAbilities).GetField(
                    "learnedAbilities",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (field == null)
                    return;

                var list = field.GetValue(comp) as List<VEF.Abilities.Ability>;
                if (list == null)
                    return;

                list.RemoveAll(a => a.def == Props.vefAbility);
            }
        }
    }

    public class HediffCompProperties_GivesAbility : HediffCompProperties
    {
        public RimWorld.AbilityDef vanillaAbility;
        public VEF.Abilities.AbilityDef vefAbility;

        public HediffCompProperties_GivesAbility()
        {
            compClass = typeof(HediffComp_GivesAbility);
        }
    }
}
