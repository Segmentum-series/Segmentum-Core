using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace seg
{
    public class CompProperties_RemovableAfterDeath : HediffCompProperties
    {
        public bool removableAfterDeath;

        public CompProperties_RemovableAfterDeath()
        {
            compClass = typeof(HediffComp_RemovableAfterDeath);
            removableAfterDeath = true;
        }
    }

    public class HediffComp_RemovableAfterDeath : HediffComp
    {
        public bool Removable
        {
            get { return ((CompProperties_RemovableAfterDeath)props).removableAfterDeath; }
        }

        public ThingDef SpawnThing
        {
            get
            {
                if (parent == null) return null;
                if (parent.def == null) return null;
                return parent.def.spawnThingOnRemoved;
            }
        }
    }

    public class CompEthicalButcheryFlag : ThingComp
    {
    }

    public static class RemovableAfterDeathUtility
    {
        public static List<Thing> Extract(Corpse corpse)
        {
            List<Thing> outList = new List<Thing>();
            if (corpse == null) return outList;
            Pawn pawn = corpse.InnerPawn;
            if (pawn == null) return outList;

            List<Hediff> list = pawn.health.hediffSet.hediffs.ToList();
            foreach (Hediff h in list)
            {
                HediffComp_RemovableAfterDeath c = h.TryGetComp<HediffComp_RemovableAfterDeath>();
                if (c == null) continue;
                if (!c.Removable) continue;

                ThingDef d = c.SpawnThing;
                if (d == null) continue;

                Thing t = ThingMaker.MakeThing(d);
                outList.Add(t);

                pawn.health.RemoveHediff(h);
                BodyPartRecord p = h.Part;
                if (p != null) pawn.health.AddHediff(HediffDefOf.MissingBodyPart, p);
            }

            return outList;
        }
    }
                [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
                public static class Patch_EthicalButchery_MakeRecipeProducts
                {
                    public static bool Prefix(
                        RecipeDef recipeDef,
                        Pawn worker,
                        List<Thing> ingredients,
                        Thing dominantIngredient,
                        IBillGiver billGiver,
                        Precept_ThingStyle precept,
                        ThingStyleDef style,
                        int? overrideGraphicIndex,
                        ref IEnumerable<Thing> __result)
                    {
                        if (recipeDef == null || recipeDef.defName != "Seg_SERV_EthicalButchery")
                            return true;

                        Corpse corpse = ingredients
                            .Select(t => t as Corpse)
                            .FirstOrDefault(c => c != null);

                        if (corpse == null)
                        {
                            __result = Enumerable.Empty<Thing>();
                            return false;
                        }

                        List<Thing> extracted = RemovableAfterDeathUtility.Extract(corpse);

                        Map map = billGiver is Thing t ? t.Map : worker.Map;
                        IntVec3 pos = billGiver is Thing bt ? bt.Position : worker.Position;



                        __result = extracted;
                        return false;
                    }
    }
        [HarmonyPatch(typeof(HistoryEventsManager), "RecordEvent")]
        public static class Patch_NoThoughts_EthicalButchery
        {
            public static bool Prefix(HistoryEvent historyEvent, bool canApplySelfTookThoughts)
            {
                HistoryEventDef def = historyEvent.def;
                if (def == null) return true;

                if (def != HistoryEventDefOf.ButcheredHuman)
                    return true;

                Pawn victim = historyEvent.args.GetArg<Pawn>(HistoryEventArgsNames.Victim);
                if (victim == null) return true;

                Corpse corpse = victim.Corpse;
                if (corpse == null) return true;

                if (corpse.GetComp<CompEthicalButcheryFlag>() != null)
                    return false;

                return true;
            }
}
}
