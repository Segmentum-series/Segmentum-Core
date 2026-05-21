using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using taranchuk_lasers;

namespace seg
{
    public class LaserProjectile_WithFilth : LaserProjectile
    {
        public override void Tick()
        {
            base.Tick();

            if (this.Destroyed || this.Map == null)
                return;

            HashSet<IntVec3> impactCells =
                GetImpactCells_Rebuilt(this.LaserProperties.damageThingsAcrossBeamLine);

            foreach (IntVec3 cell in impactCells)
            {
                if (!cell.InBounds(Map))
                    continue;

                List<Thing> things = cell.GetThingList(Map).ToList();

                foreach (Thing t in things)
                {
                    if (this.Destroyed)
                        return;

                    if (IsDamagable_Rebuilt(t))
                        TrySpawnFilthAt(t.Position);
                }
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

        private HashSet<IntVec3> GetImpactCells_Rebuilt(bool includeAcross)
        {
            HashSet<IntVec3> cells = new HashSet<IntVec3>();
            cells.AddRange<IntVec3>(GenRadial.RadialCellsAround(
                this.ExactPosition.ToIntVec3(),
                this.LaserProperties.beamWidth,
                true));

            if (includeAcross)
            {
                float dist = Vector3.Distance(this.origin.Yto0(), this.ExactPosition.Yto0());
                float step = 0f;

                while (step < dist)
                {
                    step += 0.5f;
                    IntVec3 cell = Vector3.MoveTowards(this.origin, this.ExactPosition, step).ToIntVec3();
                    if (cells.Add(cell))
                        cells.AddRange<IntVec3>(GenRadial.RadialCellsAround(cell, this.LaserProperties.beamWidth, true));
                }
            }

            return cells;
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