using System.Collections.Generic;
using RimWorld;
using Verse;
using System;


namespace Seg;

public class Hediff_SkillOffsetDisplay : Hediff_AddedPart
{
    private DefModExtension_HediffSkillOffset defMod => def?.GetModExtension<DefModExtension_HediffSkillOffset>();

        private Dictionary<SkillDef, int> appliedOffsets = new Dictionary<SkillDef, int>();

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            var mod = defMod;
            if (mod == null) yield break;
            foreach (var entry in mod.skills)
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
    public override void PostAdd(DamageInfo? dinfo)
    {
            base.PostAdd(dinfo);
            appliedOffsets.Clear();
            var mod = defMod;
            if (mod == null) return;
            foreach (var entry in mod.skills)
            {
                var skill = pawn?.skills?.GetSkill(entry.skill);
                if (skill != null)
                {
                    skill.Level += entry.offset;
                    appliedOffsets[entry.skill] = entry.offset;
                }
            }
    }

     public override void PreRemoved()
        {
            var mod = defMod;
            if (mod != null)
            {
                foreach (var kv in appliedOffsets)
                {
                    var skill = pawn?.skills?.GetSkill(kv.Key);
                    if (skill != null)
                    {
                        skill.Level = Math.Max(0, skill.Level - kv.Value);
                    }
                }
            }
            base.PreRemoved();
        }
    }

public class DefModExtension_HediffSkillOffset : DefModExtension
{
      public List<SkillOffsetEntry> skills = new List<SkillOffsetEntry>();
}

public class SkillOffsetEntry
{
    public SkillDef skill;
    public int offset;
}