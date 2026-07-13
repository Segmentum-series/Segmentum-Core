using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace seg
{
    public class Verb_LaunchProjectileStatic_UsesCharges : Verb_LaunchProjectileStatic
    {
        protected override bool TryCastShot()
        {
            bool result = base.TryCastShot();

            if (result && CasterPawn != null)
            {
                Thing equipment = EquipmentSource ?? CasterPawn.equipment?.Primary;

                if (equipment != null)
                {
                    var comp = equipment.TryGetComp<CompEquippableAbilityReloadable>();
                    if (comp != null && comp.AbilityForReading != null)
                    {
                        comp.RemainingCharges = Mathf.Max(0, comp.RemainingCharges - 1);
                        comp.UsedOnce();
                    }
                }
            }

            return result;
        }
    }

    public class CompPhotonGrenadeChargePack : ThingComp
    {
        public CompProperties_PhotonGrenadeChargePack Props => (CompProperties_PhotonGrenadeChargePack)props;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (pawn == null || pawn.Dead)
                yield break;

            var weapon = pawn.equipment?.Primary;
            if (weapon == null)
                yield break;

            var reloadComp = weapon.TryGetComp<CompEquippableAbilityReloadable>();
            if (reloadComp == null || !reloadComp.NeedsReload(false))
                yield break;

            yield return new FloatMenuOption(
                "Reload " + weapon.LabelShort,
                () =>
                {
                    var job = JobMaker.MakeJob(JobDefOf.Reload, weapon, parent);
                    job.count = 1; // how many packs to use
                    pawn.jobs.TryTakeOrderedJob(job);
                }
            );
        }
    }

    public class CompProperties_PhotonGrenadeChargePack : CompProperties
    {
        public int ammoCountToRefill = 1;

        public CompProperties_PhotonGrenadeChargePack()
        {
            compClass = typeof(CompPhotonGrenadeChargePack);
        }
    }
      public class Verb_LaunchProjectileBurst_UsesCharges : Verb_Shoot
    {
        protected override bool TryCastShot()
        {
            bool result = base.TryCastShot();

            if (result && CasterPawn != null)
            {
                Thing equipment = EquipmentSource ?? CasterPawn.equipment?.Primary;

                if (equipment != null)
                {
                    var comp = equipment.TryGetComp<CompEquippableAbilityReloadable>();
                    if (comp != null && comp.AbilityForReading != null)
                    {
                        comp.RemainingCharges = Mathf.Max(0, comp.RemainingCharges - 1);
                        comp.UsedOnce();
                    }
                }
            }

            return result;
        }
    }
}