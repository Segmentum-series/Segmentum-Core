using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using taranchuk_lasers;

namespace seg
{
    [HarmonyPatch(typeof(LaserProjectile), "Launch")]
    public static class OverheatPatch_LaserLaunch
    {
        static void Postfix(
            LaserProjectile __instance,
            Thing launcher,
            Vector3 origin,
            LocalTargetInfo usedTarget,
            LocalTargetInfo intendedTarget,
            ProjectileHitFlags hitFlags,
            bool preventFriendlyFire,
            Thing equipment,
            ThingDef targetCoverDef)
        {
            Pawn caster = launcher as Pawn;
            if (caster == null)
                return;

            // LaserProjectile does NOT initialize comps, so we must do it manually
            if (__instance.AllComps == null)
                __instance.InitializeComps();

            CompProjectileOverheat comp = __instance.GetComp<CompProjectileOverheat>();
            if (comp == null)
                return;

            comp.TryOverheat(caster);
        }
    }

    public class CompProperties_ProjectileOverheat : CompProperties
    {
        public float overheatChance = 0.05f;
        public float blastRadius = 2f;
        public string damageDef = "Flame";
        public int damageAmount = 10;

        public CompProperties_ProjectileOverheat()
        {
            this.compClass = typeof(CompProjectileOverheat);
        }
    }

    public class CompProjectileOverheat : ThingComp
{
    public CompProperties_ProjectileOverheat Props =>
        (CompProperties_ProjectileOverheat)this.props;

    public void TryOverheat(Pawn caster)
{
    if (Rand.Value > Props.overheatChance)
        return;

    
    GenExplosion.DoExplosion(
        caster.Position,
        caster.Map,
        Props.blastRadius,
        DefDatabase<DamageDef>.GetNamed(Props.damageDef),
        caster,
        Props.damageAmount
    );

    // Custom fleck (your plasma splatter)
    FleckDef fleck = DefDatabase<FleckDef>.GetNamed("SEG_COTO_Plasmafleksplatter", false);
    if (fleck != null)
    {
        FleckMaker.Static(
            caster.Position,
            caster.Map,
            fleck,
            1.5f
        );
    }
}
}
}
