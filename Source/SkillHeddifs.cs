 using RimWorld;
using Verse;
using System.Text;
using System.Linq;
using Core40k;
using System.Collections.Generic;
using HarmonyLib;

 

namespace Seg{

 public class HediffComp_SkillOffset : HediffComp
    {
        private bool applied = false;

        public HediffCompProperties_SkillOffset Props =>
            (HediffCompProperties_SkillOffset)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            Apply();
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            Apply();
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            Remove();
        }

        private void Apply()
        {
            if (applied) return;

            foreach (var entry in Props.skills)
            {
                var skill = Pawn.skills?.GetSkill(entry.skill);
                if (skill != null)
                    skill.Level += entry.offset;
            }

            applied = true;
        }

        private void Remove()
        {
            if (!applied) return;

            foreach (var entry in Props.skills)
            {
                var skill = Pawn.skills?.GetSkill(entry.skill);
                if (skill != null)
                    skill.Level -= entry.offset;
            }

            applied = false;
        }
    }

public class SkillOffsetEntry
    {
        public SkillDef skill;
        public int offset;
    }

    public class HediffCompProperties_SkillOffset : HediffCompProperties
    {
        public List<SkillOffsetEntry> skills = new List<SkillOffsetEntry>();

        public HediffCompProperties_SkillOffset()
        {
            compClass = typeof(HediffComp_SkillOffset);
        }
    }
     public class Hediff_SkillOffsetDisplay : HediffWithComps
    {
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (var comp in comps)
            {
                if (comp is HediffComp_SkillOffset skillComp)
                {
                    foreach (var entry in skillComp.Props.skills)
                    {
                        yield return new StatDrawEntry(
                            StatCategoryDefOf.BasicsPawn,
                            $"{entry.skill.label} skill offset",
                            $"+{entry.offset}",
                            $"This implant increases {entry.skill.label} skill by {entry.offset}.",
                            1000
                        );
                    }
                }
            }
        }
    }
      public class ThoughtWorker_DivineSilicion : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (p.Map == null)
                return ThoughtState.Inactive;

            int count = 0;

            foreach (Building b in p.Map.listerBuildings.allBuildingsColonist)
            {
                ThingDef d = b.def;
                if (d == null)
                    continue;

                var tc = d.thingClass;
                if (tc == null)
                    continue;

                if (typeof(Building_WorkTable).IsAssignableFrom(tc) ||
                    typeof(Building_WorkTable_HeatPush).IsAssignableFrom(tc) ||
                    typeof(Building_ResearchBench).IsAssignableFrom(tc) ||
                    typeof(Building_PlantGrower).IsAssignableFrom(tc) ||
                    typeof(Building_NutrientPasteDispenser).IsAssignableFrom(tc))
                {
                    count++;
                }
            }

            if (count == 0) return ThoughtState.ActiveAtStage(0);
            if (count <= 2) return ThoughtState.ActiveAtStage(1);
            if (count <= 4) return ThoughtState.ActiveAtStage(2);
            return ThoughtState.ActiveAtStage(3);
        }
    }

[StaticConstructorOnStartup]
    public static class DivineSilicion_Harmony
    {
        static DivineSilicion_Harmony()
        {
            new Harmony("Seg.DivineSilicion").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
    public static class DivineSilicion_SpawnPatch
    {
        static void Postfix(Thing __instance)
        {
            if (__instance is Building b)
                DivineSilicion_RefreshUtil.RefreshColonists(b.Map);
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.DeSpawn))]
    public static class DivineSilicion_DespawnPatch
    {
        static void Prefix(Thing __instance)
        {
            if (__instance is Building b)
                DivineSilicion_RefreshUtil.RefreshColonists(b.Map);
        }
    }

    public static class DivineSilicion_RefreshUtil
    {
        public static void RefreshColonists(Map map)
        {
            if (map == null)
                return;

            List<Pawn> pawns = map.mapPawns.FreeColonistsSpawned;

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                p.needs?.mood?.thoughts?.situational?.Notify_SituationalThoughtsDirty();
            }
        }
    }
}