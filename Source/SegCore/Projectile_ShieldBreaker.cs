using RimWorld;
using Verse;

namespace seg
{
        public class Projectile_ShieldBreaker : Projectile
    {
        private static readonly DamageDef ShieldBreakerExplosion = DefDatabase<DamageDef>.GetNamed("Seg_VIND_ShieldBreakerExplosion");
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Pawn pawn = hitThing as Pawn;

            if (pawn != null)
            {
                // Detect both shield types
                CompShield shield = FindShieldOnPawn(pawn);

                bool hasShield = 
                    (shield != null && shield.ShieldState == ShieldState.Active);

                // 1. SHIELD PRESENT → break + explode
                if (hasShield)
                {
                    // EMP breaks both shield types
                    pawn.TakeDamage(new DamageInfo(
                        DamageDefOf.EMP,
                        1,
                        0,
                        this.ExactRotation.eulerAngles.y,
                        this.launcher
                    ));

                    // Explosion that hurts mechs AND pawns
                    GenExplosion.DoExplosion(
                        pawn.Position,
                        pawn.Map,
                        2.5f,
                        ShieldBreakerExplosion,
                        this.launcher,
                        20,
                        armorPenetration: 1.0f
                    );

                    this.Destroy(DestroyMode.Vanish);
                    return;
                }

                // 2. NO SHIELD → apply normal damage
                int dmg = this.def.projectile.GetDamageAmount(this.launcher);
                float ap = this.def.projectile.GetArmorPenetration(this.launcher);

                pawn.TakeDamage(new DamageInfo(
                    this.def.projectile.damageDef,
                    dmg,
                    ap,
                    this.ExactRotation.eulerAngles.y,
                    this.launcher,
                    null,
                    this.equipmentDef
                ));

                this.Destroy(DestroyMode.Vanish);
                return;
            }

            base.Impact(hitThing, blockedByShield);
        }

        private CompShield FindShieldOnPawn(Pawn pawn)
        {
            CompShield shield = pawn.GetComp<CompShield>();
            if (shield != null)
                return shield;

            // apparel shields
            if (pawn.apparel != null)
            {
                foreach (var app in pawn.apparel.WornApparel)
                {
                    var comp = app.GetComp<CompShield>();
                    if (comp != null)
                        return comp;
                }
            }

            return null;
        }
    }
}