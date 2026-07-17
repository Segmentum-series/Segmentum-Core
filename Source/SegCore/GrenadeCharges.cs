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
    public class CompProperties_AbilityLaunchProjectileBurst : CompProperties_AbilityLaunchProjectile
{
    public int burstCount = 1;
    public float ticksBetweenShots = 10f;

    public CompProperties_AbilityLaunchProjectileBurst()
    {
        this.compClass = typeof(CompAbilityEffect_LaunchProjectileBurst);
    }
}
public class CompAbilityEffect_LaunchProjectileBurst : CompAbilityEffect_WithDuration
{
    public new CompProperties_AbilityLaunchProjectileBurst Props
        => (CompProperties_AbilityLaunchProjectileBurst)this.props;

    private int shotsFired;
    private int nextShotTick;
    private LocalTargetInfo storedTarget;
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
            {
                base.Apply(target, dest);

                storedTarget = target;
                shotsFired = 0;
                nextShotTick = Find.TickManager.TicksGame;

                int totalBurstTicks = (int)(Props.ticksBetweenShots * (Props.burstCount - 1)) + 1;

                if (totalBurstTicks < 1)
                    totalBurstTicks = 1;
                parent.pawn.stances.SetStance(new Stance_Warmup(totalBurstTicks, storedTarget, parent.verb));
            }
    public override void CompTick()
                {
                    base.CompTick();

                    if (storedTarget.IsValid)
                    {
                        parent.pawn.rotationTracker.FaceCell(storedTarget.Cell);
                    }

                    if (shotsFired >= Props.burstCount)
                        return;

                    int currentTick = Find.TickManager.TicksGame;

                    if (currentTick >= nextShotTick)
                    {
                        LaunchProjectile(storedTarget);
                        shotsFired++;

                        nextShotTick = currentTick + (int)Props.ticksBetweenShots;
                    }
                }

    private void LaunchProjectile(LocalTargetInfo target)
    {
        if (Props.projectileDef == null)
            return;

        Pawn pawn = parent.pawn;

        Projectile proj = (Projectile)GenSpawn.Spawn(
            Props.projectileDef,
            pawn.Position,
            pawn.Map
        );

        proj.Launch(
            pawn,
            pawn.DrawPos,
            target,
            target,
            ProjectileHitFlags.IntendedTarget,
            parent.verb.preventFriendlyFire
        );
    }
}
}