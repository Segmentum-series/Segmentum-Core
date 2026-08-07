using Core40k;
using RimWorld;
using Verse;
using System.Linq;
using Verse.AI;

namespace seg
{
    public class PawnRenderNode_AttachmentAccessory : PawnRenderNode
    {
        public PawnRenderNode_AttachmentAccessory(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            string backpackPath = Props.texPath;

            var worn = pawn.apparel.WornApparel;

            ThingDef cargoPack  = DefDatabase<ThingDef>.GetNamed("Seg_Apparel_CargoPack",     false);
            ThingDef medicaeBag = DefDatabase<ThingDef>.GetNamed("Seg_Apparel_MedicaeBag",    false);
            ThingDef powerPack  = DefDatabase<ThingDef>.GetNamed("Seg_Apparel_PowerPack",     false);
            ///ThingDef voxCaster  = DefDatabase<ThingDef>.GetNamed("Seg_Apparel_VoxCaster",     false);
            ThingDef jumpPack   = DefDatabase<ThingDef>.GetNamed("Apparel_PackJump",          false);

            bool hasCargo   = cargoPack  != null && worn.Any(a => a.def == cargoPack);
            bool hasMedical = medicaeBag != null && worn.Any(a => a.def == medicaeBag);
            bool hasPower   = powerPack  != null && worn.Any(a => a.def == powerPack);
            ///bool hasComm    = voxCaster  != null && worn.Any(a => a.def == voxCaster);
            bool hasJump    = jumpPack   != null && worn.Any(a => a.def == jumpPack);

            if (ModsConfig.RoyaltyActive && hasJump)
                backpackPath += "_Jump";

            if (hasCargo)
                backpackPath += "_cargo";

            if (hasMedical)
                backpackPath += "_medical";

            if (hasPower)
                backpackPath += "_power";

            ///if (hasComm)
               /// backpackPath += "_communication";

            if (hasCargo && backpackPath.EndsWith("_cargo"))
                cargoPack.graphicData.texPath = "";

            if (hasMedical && backpackPath.EndsWith("_medical"))
                medicaeBag.graphicData.texPath = "";

            if (hasPower && backpackPath.EndsWith("_power"))
                powerPack.graphicData.texPath = "";

            ///if (hasComm && backpackPath.EndsWith("_communication"))
               ///voxCaster.graphicData.texPath = "";

            return GraphicDatabase.Get<Graphic_Multi>(
                backpackPath,
                ShaderFor(pawn),
                Props.drawSize,
                apparel.DrawColor,
                apparel.DrawColorTwo
            );
        }
    }
}