using RimWorld;
using Verse;
using System.Text;
using System.Linq;
using Core40k;
using System.Collections.Generic;

namespace Seg
{
    public class RankDef : Core40k.RankDef
    {
        public int ImplantsRequired = 0;
        public HediffDef RequiredHediff;
        public List<HediffDef> RequiredHediffs = new List<HediffDef>();

        public override bool RequirementMet(
            StringBuilder sb,
            Pawn pawn,
            CompRankInfo rankComp,
            RankCategoryDef currentCategory,
            out string reason)
        {
            bool valid = true;

            if (ImplantsRequired > 0)
            {
                int installed = pawn.health.hediffSet.hediffs
                    .Count(h => h is Hediff_AddedPart || h is Hediff_Implant);

                if (installed < ImplantsRequired)
                {
                    valid = false;
                    sb.AppendLine($"Requires at least {ImplantsRequired} artificial parts (has {installed}).");
                }
            }

            if (RequiredHediff != null)
            {
                if (!pawn.health.hediffSet.HasHediff(RequiredHediff))
                {
                    valid = false;
                    sb.AppendLine($"Requires Implant: {RequiredHediff.label.CapitalizeFirst()}.");
                }
            }

            if (RequiredHediffs != null && RequiredHediffs.Count > 0)
            {
                foreach (var req in RequiredHediffs)
                {
                    if (req == null) continue;
                    if (!pawn.health.hediffSet.HasHediff(req))
                    {
                        valid = false;
                        sb.AppendLine($"Requires Implant: {req.label.CapitalizeFirst()}.");
                    }
                }
            }

            bool baseResult = base.RequirementMet(sb, pawn, rankComp, currentCategory, out reason);
            return valid && baseResult;
        }

        public override string BuildRankBonusString(StringBuilder sb)
        {
            return base.BuildRankBonusString(sb);
        }

        public override void UnlockRank(CompRankInfo rankComp)
        {
            base.UnlockRank(rankComp);
        }

        public override void RemoveRank(CompRankInfo rankComp)
        {
            base.RemoveRank(rankComp);
        }
    }
}
