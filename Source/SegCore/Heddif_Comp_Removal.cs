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
    public class CompServitorFlag : ThingComp 
    {public bool servitor = false;

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref servitor, "servitor", false);
    }}
    public class CompServitorCorpseFlag : ThingComp
    {
        public bool servitor = false;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref servitor, "servitor", false);
        }
    }
    public class CompProperties_ServitorCorpseFlag : CompProperties
    {
        public CompProperties_ServitorCorpseFlag()
        {
            this.compClass = typeof(CompServitorCorpseFlag);
        }
    }
            [StaticConstructorOnStartup]
        public static class ServitorCorpseCompInjector
        {
            static ServitorCorpseCompInjector()
            {
                var corpseDefs = DefDatabase<ThingDef>.AllDefs
                    .Where(td => td.thingClass != null && typeof(Corpse).IsAssignableFrom(td.thingClass));

                foreach (var def in corpseDefs)
                {
                    if (def.comps == null)
                        def.comps = new List<CompProperties>();

                    def.comps.Add(new CompProperties_ServitorCorpseFlag());
                }
            }
        }
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.MakeCorpse))]
    [HarmonyPatch(new Type[] { typeof(Building_Grave), typeof(bool), typeof(float) })]
    public static class Patch_MakeCorpse_MarkServitor
    {
        public static void Postfix(Pawn __instance, Corpse __result)
        {
            if (__result == null)
            {
                Log.Message("[ks-serv-debug] MakeCorpse postfix fired but __result was null.");
                return;
            }

            bool isServitor = __instance.health.hediffSet.hediffs
                .Any(h => h.TryGetComp<HediffComp_RemovableAfterDeath>()?.Removable == true);

            Log.Message($"[ks-serv-debug] MakeCorpse fired for pawn {__instance.Name} | Hediff says servitor = {isServitor}");

            if (!isServitor)
            {
                Log.Message("[ks-serv-debug]not servitor.");
                return;
            }

            var comp = __result.TryGetComp<CompServitorCorpseFlag>();
            if (comp == null)
            {
                Log.Message("[ks-serv-debug] MakeCorpse fired but CompServitorCorpseFlag was null on the corpse.");
                return;
            }

            comp.servitor = true;
            Log.Message("[ks-serv-debug] Servitor parts found, marking servitor corpse.");
        }
    }
      public class CompProperties_EthicalButcheryFlag : CompProperties
    {
        public CompProperties_EthicalButcheryFlag()
        {
            this.compClass = typeof(CompEthicalButcheryFlag);
        }
    }
   public class SpecialThingFilterWorker_ServitorCorpses : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            var comp = t.TryGetComp<CompServitorCorpseFlag>();
            return comp?.servitor == true;
        }

        public override bool CanEverMatch(ThingDef def)
        {
            bool can = typeof(Corpse).IsAssignableFrom(def.thingClass);
            return can;
        }
    }
    public class SpecialThingFilterWorker_NonServitorCorpses : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t)
        {
            var comp = t.TryGetComp<CompServitorCorpseFlag>();
            return comp == null || comp.servitor == false;
        }

        public override bool CanEverMatch(ThingDef def)
        {
            return typeof(Corpse).IsAssignableFrom(def.thingClass);
        }
    }
                   
              [StaticConstructorOnStartup]
            public static class Patch_RemoveServitorChipRecipe
            {
                static Patch_RemoveServitorChipRecipe()
                {
                    var bad = new HashSet<string>
                    {
                        "RemoveImplant_Seg_ServitorChip",
                        "RemoveImplantt_Seg_Servitors_MedicaeChip",
                        "RemoveImplant_Seg_Servitors_CombatChip",
                        "RemoveImplant_Seg_Servitors_LexomatChip",
                        "RemoveHediff_Seg_Servitors_ServitorizationHediff",
                        "RemoveHediff_Seg_Servitors_MedicaeServitorHediff",
                        "RemoveHediff_Seg_Servitors_CombatServitorHediff",
                        "RemoveHediff_Seg_Servitors_LexomatServitorHediff"
                    };

                    Log.Message($"[ks-serv-debug] Removing {bad.Count} recipes.");

                    foreach (var defName in bad)
                    {
                        var recipe = DefDatabase<RecipeDef>.GetNamedSilentFail(defName);

                        if (recipe != null)
                        {
                            DefDatabase<RecipeDef>.AllDefsListForReading.Remove(recipe);
                            Log.Message($"[ks-serv-debug] Removed {recipe.defName}");
                        }
                        else
                        {
                            Log.Message($"[ks-serv-debug] Not found: {defName}");
                        }
                    }
                }
            }

            [StaticConstructorOnStartup]
            public static class SegServitorRecipeDump
            {
                static SegServitorRecipeDump()
                {
                    foreach (var r in DefDatabase<RecipeDef>.AllDefs)
                    {
                        if (r.defName.Contains("Seg_Servitors") || r.defName.Contains("Servitor"))
                            Log.Message($"[ks-serv-debug] RecipeDef: {r.defName}");
                    }
                }
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
