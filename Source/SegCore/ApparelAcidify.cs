using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;
using System.Linq;

namespace seg
{
[HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Patch_Pawn_Kill
    {
        public static void Prefix(Pawn __instance)
        {
            if (__instance?.apparel == null)
                return;

            var worn = __instance.apparel.WornApparel.ToList();
            foreach (var app in worn)
            {
                var comp = app.GetComp<Comp_VanishOnDeath>();
                if (comp != null)
                {
                    comp.DoVanishEffect(__instance);
                    app.Destroy(DestroyMode.Vanish);
                }
            }
        }
    } public class CompProperties_VanishOnDeath : CompProperties
    {
        public FleckDef fleck;
        public ThingDef mote;
        public int moteCount = 3;
        public FloatRange moteOffsetRange = new FloatRange(0.2f, 0.4f);
        public ThingDef filth;
        public int filthCount = 4;

        public CompProperties_VanishOnDeath()
        {
            compClass = typeof(Comp_VanishOnDeath);
        }
    }

    public class Comp_VanishOnDeath : ThingComp
    {
        public CompProperties_VanishOnDeath Props =>
            (CompProperties_VanishOnDeath)props;

        public void DoVanishEffect(Pawn pawn)
        {
            if (!pawn.Spawned)
                return;

            Vector3 drawPos = pawn.DrawPos;

            if (Props.mote != null || Props.fleck != null)
            {
                for (int i = 0; i < Props.moteCount; i++)
                {
                    Vector2 offset = Rand.InsideUnitCircle * Props.moteOffsetRange.RandomInRange;
                    Vector3 loc = new Vector3(drawPos.x + offset.x, drawPos.y, drawPos.z + offset.y);

                    if (Props.mote != null)
                        MoteMaker.MakeStaticMote(loc, pawn.Map, Props.mote);
                    else
                        FleckMaker.Static(loc, pawn.Map, Props.fleck);
                }
            }

            if (Props.filth != null)
                FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, Props.filth, Props.filthCount);
        }
    }
}