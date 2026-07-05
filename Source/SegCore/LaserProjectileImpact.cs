using RimWorld;
using Verse;
using UnityEngine;
using System.Linq;
using taranchuk_lasers;

namespace seg
{
  public class LaserProjectile_WithImpactEffecter : LaserProjectile
    {
        private Effecter impactEffecter;

        public override void Tick()
        {
            base.Tick();

            if (Destroyed || Map == null)
                return;

            IntVec3 impactCell = ExactPosition.ToIntVec3();
            if (!impactCell.InBounds(Map))
                return;

            var lp = this.LaserProperties as LaserProperties_WithImpactEffecter;
            if (lp?.impactEffecter != null && impactEffecter == null)
            {
                Vector3 offset = ExactPosition - impactCell.ToVector3Shifted();
                impactEffecter = lp.impactEffecter.Spawn(impactCell, Map, offset);
            }

            if (impactEffecter != null)
            {
                Vector3 offset = ExactPosition - impactCell.ToVector3Shifted();
                impactEffecter.offset = offset;
                impactEffecter.EffectTick(new TargetInfo(impactCell, Map), TargetInfo.Invalid);
                impactEffecter.ticksLeft--;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (impactEffecter != null)
            {
                impactEffecter.Cleanup();
                impactEffecter = null;
            }

            base.Destroy(mode);
        }
    }

    public class LaserProperties_WithImpactEffecter : LaserProperties
    {
        public EffecterDef impactEffecter;
    }
}