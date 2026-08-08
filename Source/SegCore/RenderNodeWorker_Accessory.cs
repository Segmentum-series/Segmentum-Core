using System.Linq;
using Core40k;
using RimWorld;
using Verse;
using UnityEngine;

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

            ThingDef cargoPack  = DefDatabase<ThingDef>.GetNamed("Seg_Apparel_CargoPack",  false);
            ThingDef medicaeBag = DefDatabase<ThingDef>.GetNamed("Seg_Apparel_MedicaeBag", false);
            ThingDef powerPack  = DefDatabase<ThingDef>.GetNamed("Seg_Apparel_PowerPack",  false);
            ThingDef jumpPack   = DefDatabase<ThingDef>.GetNamed("Apparel_PackJump",       false);

            bool hasCargo   = cargoPack  != null && worn.Any(a => a.def == cargoPack);
            bool hasMedical = medicaeBag != null && worn.Any(a => a.def == medicaeBag);
            bool hasPower   = powerPack  != null && worn.Any(a => a.def == powerPack);
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


            var multiColor = apparel.GetComp<CompMultiColor>();

            string maskPath = null;
            if (multiColor?.MaskDef != null)
            {
                maskPath = multiColor.MaskDef.maskPath;
                if (multiColor.MaskDef.useBodyTypes && pawn.story?.bodyType != null)
                    maskPath += "_" + pawn.story.bodyType.defName;
            }

            Shader shader = apparel.def.graphicData.shaderType.Shader;
            if (multiColor?.Props != null && multiColor.Props.colorMaskAmount == 3)
                shader = Core40kDefOf.BEWH_CutoutThreeColor.Shader;

            return MultiColorUtils.GetGraphic<Graphic_Multi>(
                backpackPath,
                shader,
                Props.drawSize,
                multiColor?.DrawColor       ?? apparel.DrawColor,
                multiColor?.DrawColorTwo    ?? apparel.DrawColorTwo,
                multiColor?.DrawColorThree  ?? apparel.DrawColorTwo,
                apparel.def.graphicData,
                maskPath
            );
        }
    }
}