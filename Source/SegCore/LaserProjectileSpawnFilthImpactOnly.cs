using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using taranchuk_lasers;

namespace seg
{
    public class LaserProjectile_WithFilthImpactOnly : LaserProjectile
    {
        public override void Tick()
        {
            base.Tick();

            if (this.Destroyed || this.Map == null)
                return;

            IntVec3 impactCell = this.ExactPosition.ToIntVec3();

            if (!impactCell.InBounds(Map))
                return;

            List<Thing> things = impactCell.GetThingList(Map).ToList();

            foreach (Thing t in things)
            {
                if (this.Destroyed)
                    return;

                if (IsDamagable_Rebuilt(t))
                    TrySpawnFilthAt(impactCell);
            }
        }

        private void TrySpawnFilthAt(IntVec3 cell)
        {
            ProjectileProperties props = def.projectile;

            if (props.filth != null && Rand.Value < props.filthChance)
            {
                int count = props.filthCount.RandomInRange;
                for (int i = 0; i < count; i++)
                    FilthMaker.TryMakeFilth(cell, Map, props.filth);
            }

            if (props.spawnsThingDef != null)
            {
                Thing thing = ThingMaker.MakeThing(props.spawnsThingDef);
                GenPlace.TryPlaceThing(thing, cell, Map, ThingPlaceMode.Near);
            }
        }

        private bool IsDamagable_Rebuilt(Thing thing)
        {
            if ((thing is Pawn || thing.def.useHitPoints) && thing != this.launcher)
            {
                switch (thing)
                {
                    case Projectile _:
                    case Filth _:
                        break;
                    default:
                        return !(thing is Mote);
                }
            }
            return false;
        }
    }
}