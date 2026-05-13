using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Seg;

public class Hediff_SkillOffsetDisplay : HediffWithComps
{
    private DefModExtension_HediffSkillOffset defMod => def.GetModExtension<DefModExtension_HediffSkillOffset>();
    
    public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
    {
        foreach (var entry in defMod.skills)
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
        foreach (var entry in defMod.skills)
        {
            var skill = pawn.skills?.GetSkill(entry.skill);
            skill?.Level += entry.offset;
        }
        base.PostAdd(dinfo);
    }

    public override void PreRemoved()
    {
        foreach (var entry in defMod.skills)
        {
            var skill = pawn.skills?.GetSkill(entry.skill);
            skill?.Level -= entry.offset;
        }
        base.PostRemoved();
    }
}

public class DefModExtension_HediffSkillOffset : DefModExtension
{
    public List<SkillOffsetEntry> skills = [];
}

public class SkillOffsetEntry
{
    public SkillDef skill;
    public int offset;
}