using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Seg;

public class ArmTextureExtension : DefModExtension;

public class PawnRenderNodeWorker_Arm : PawnRenderNodeWorker
{
    public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
    {
        var baseVector = base.OffsetFor(node, parms, out pivot);
        if ((SegCoreUtils.ShouldFlipArmGraphic(node.hediff) && parms.pawn.Rotation == Rot4.West) || (!SegCoreUtils.ShouldFlipArmGraphic(node.hediff) && parms.pawn.Rotation == Rot4.East))
        {
            baseVector.y += 1;
        }

        return baseVector;
    }

    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        if (SegCoreUtils.ShouldFlipArmGraphic(node.hediff))
        {
            node.Props.side = PawnRenderNodeProperties.Side.Left;   
        }
        
        if (!node.Props.rotDrawMode.HasFlag(parms.rotDrawMode))
        {
            return false;
        }
        if (node.Props.visibleFacing != null && !node.Props.visibleFacing.Contains(parms.facing))
        {
            return false;
        }
        if (node.Props.skipFlag != RenderSkipFlagDefOf.None && parms.skipFlags.HasFlag(node.Props.skipFlag))
        {
            return false;
        }
        if (node.bodyPart?.visibleHediffRots != null && !node.bodyPart.visibleHediffRots.Contains(parms.facing))
        {
            return false;
        }
        if (node.Props.linkedBodyPartsGroup != null && !parms.pawn.health.hediffSet.GetNotMissingParts().Any(x => x.groups.NotNullAndContains(node.Props.linkedBodyPartsGroup)))
        {
            return false;
        }
        return node.DebugEnabled;
    }
}

public class PawnRenderNode_Arm : PawnRenderNode
{
    public PawnRenderNode_Arm(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
    {
    }

    public override bool FlipGraphic(PawnDrawParms parms)
    {
        var shouldFlip = SegCoreUtils.ShouldFlipArmGraphic(hediff);
        if (shouldFlip && (parms.facing == Rot4.West || parms.facing == Rot4.East))
        {
            shouldFlip = false;
        }
        
        return shouldFlip;
    }
}