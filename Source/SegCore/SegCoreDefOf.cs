using RimWorld;
using Verse;

namespace Seg;

[DefOf]
public class SegCoreDefOf
{
    public static HediffDef SEG_RadBuildup;
    
    static SegCoreDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(SegCoreDefOf));
    }
}