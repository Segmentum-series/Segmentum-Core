using UnityEngine;
using Verse;

namespace Seg;

public class ArmTextureExtension : DefModExtension;

public class PawnRenderNodeWorker_Arm : PawnRenderNodeWorker
{
    public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
    {
        var baseVector = base.OffsetFor(node, parms, out pivot);
        if (SegCoreUtils.AlreadyHasArmHediff(parms.pawn))
        {
            baseVector.y += 1;
        }

        return baseVector;
    }

    public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
    {
        if (SegCoreUtils.ShouldFlipArmGraphic(parms.pawn, node.hediff))
        {
            node.Props.side = PawnRenderNodeProperties.Side.Left;
        }
        return base.CanDrawNow(node, parms);
    }
}

public class PawnRenderNode_Arm : PawnRenderNode
{
    public PawnRenderNode_Arm(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree)
    {
    }

    public override bool FlipGraphic(PawnDrawParms parms)
    {
        var shouldFlip = SegCoreUtils.ShouldFlipArmGraphic(parms.pawn, hediff);
        if (shouldFlip && parms.facing == Rot4.West)
        {
            //Due to how flipping works it'll flip wrongly on the west side so we need to adjust for that specific pawn facing
            shouldFlip = false;
        }
        
        return shouldFlip;
    }
}