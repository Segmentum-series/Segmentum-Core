using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
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
            if (__instance.AllComps == null)
                __instance.InitializeComps();

            CompProjectileOverheat comp = __instance.GetComp<CompProjectileOverheat>();
            if (comp == null)
                return;

            comp.TryOverheat(caster);
        }
    }
    [HarmonyPatch(
    typeof(Projectile),
    nameof(Projectile.Launch),
    new Type[] {
        typeof(Thing),
        typeof(Vector3),
        typeof(LocalTargetInfo),
        typeof(LocalTargetInfo),
        typeof(ProjectileHitFlags),
        typeof(bool),
        typeof(Thing),
        typeof(ThingDef)
    }
)]
public static class OverheatPatch_AllProjectiles
{
    static void Postfix(
        Projectile __instance,
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

        if (__instance.AllComps == null)
            __instance.InitializeComps();

        var comp = __instance.GetComp<CompProjectileOverheat>();
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

        public string fleckDefName = null;

        public CompProperties_ProjectileOverheat()
        {
            this.compClass = typeof(CompProjectileOverheat);
        }
    }
    public class CompProjectileOverheat : ThingComp
{
    public CompProperties_ProjectileOverheat Props =>
        (CompProperties_ProjectileOverheat)this.props;

    private List<int> recentFireTicks = new List<int>();

    public void RegisterFire()
    {
        int now = Find.TickManager.TicksGame;
        recentFireTicks.Add(now);
        recentFireTicks.RemoveAll(t => now - t > 600);
    }

    public void TryOverheat(Pawn caster)
    {
        RegisterFire();

        int timesFiredRecently = recentFireTicks.Count;
        float finalChance = Props.overheatChance * timesFiredRecently;
        if (Rand.Value > finalChance)
            return;
        GenExplosion.DoExplosion(
            caster.Position,
            caster.Map,
            Props.blastRadius,
            DefDatabase<DamageDef>.GetNamed(Props.damageDef),
            caster,
            Props.damageAmount
        );
        if (!Props.fleckDefName.NullOrEmpty())
        {
            FleckDef fleck = DefDatabase<FleckDef>.GetNamed(Props.fleckDefName, false);
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
}