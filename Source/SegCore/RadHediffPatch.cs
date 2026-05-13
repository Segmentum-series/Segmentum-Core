using Verse;

namespace Seg;

public class SEG_Core_Rad_Verb_Shoot : Verb_Shoot
{
    protected override bool TryCastShot()
    {
        var fired = base.TryCastShot();
        if (!fired)
        {
            return false;
        }
        
        if (!CasterIsPawn)
        {
            return true;
        }

        var hediff = CasterPawn.health.hediffSet.GetFirstHediffOfDef(SegCoreDefOf.SEG_RadBuildup);
        if (hediff == null)
        {
            hediff = HediffMaker.MakeHediff(SegCoreDefOf.SEG_RadBuildup, CasterPawn);
            CasterPawn.health.AddHediff(hediff);
        }

        hediff.Severity += 0.001f;

        return true;
    }
}