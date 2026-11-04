using RimWorld;
using Verse;
using System.Text;
using System.Linq;
using Core40k;
using System.Collections.Generic;

namespace Seg
{
   public class PawnRequirement_ArtificialParts : PawnRequirement
{
    public int count;

    public override bool IsEmpty => count <= 0;

    public override bool Describe(RequirementContext context, RequirementReport report)
    {
        var installed = Installed(context.pawn);
        report.Header("Artificial parts");
        report.Line($"At least {count} (has {installed})", installed >= count);
        return installed >= count;
    }

    public override bool IsMet(RequirementContext context) => Installed(context.pawn) >= count;

    private static int Installed(Pawn pawn)
    {
        var installed = 0;
        foreach (var hediff in pawn?.health?.hediffSet?.hediffs ?? [])
        {
            if (hediff is Hediff_AddedPart or Hediff_Implant)
            {
                installed++;
            }
        }
        return installed;
    }
}
}