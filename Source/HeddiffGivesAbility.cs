 using RimWorld;
using Verse;
using System.Text;
using System.Linq;
using Core40k;
using System.Collections.Generic;
 namespace Seg{
    public class HediffComp_GivesAbility : HediffComp
{
    public HediffCompProperties_GivesAbility Props => (HediffCompProperties_GivesAbility)props;

    public override void CompPostPostAdd(DamageInfo? dinfo)
    {
        base.CompPostPostAdd(dinfo);
        Pawn.abilities?.GainAbility(Props.ability);
    }

    public override void CompPostPostRemoved()
    {
        base.CompPostPostRemoved();
        Pawn.abilities?.RemoveAbility(Props.ability);
    }
}

public class HediffCompProperties_GivesAbility : HediffCompProperties
{
    public AbilityDef ability;

    public HediffCompProperties_GivesAbility()
    {
        compClass = typeof(HediffComp_GivesAbility);
    }
}
 }