using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Seg
{
    public class ArmTextureExtension : DefModExtension
    {
        public bool isArmTexture = true;
    }

    public class PawnRenderNodeWorker_Arm : PawnRenderNodeWorker
    {
    }

    public class PawnRenderNode_Arm : PawnRenderNode
    {
        public PawnRenderNode_Arm(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override bool FlipGraphic(PawnDrawParms parms)
        {
            var ext = this.hediff?.def?.GetModExtension<ArmTextureExtension>();
            if (ext?.isArmTexture != true)
                return base.FlipGraphic(parms);

            if (HasEarlierArmNode(this.tree.rootNode, this))
                return true;

            return base.FlipGraphic(parms);
        }

        private bool HasEarlierArmNode(PawnRenderNode root, PawnRenderNode current)
        {
            bool seenCurrent = false;
            return HasEarlierArmNodeRecursive(root, current, ref seenCurrent);
        }

        private bool HasEarlierArmNodeRecursive(PawnRenderNode node, PawnRenderNode current, ref bool seenCurrent)
        {
            if (node == current)
            {
                seenCurrent = true;
            }
            else
            {
                var ext = node.hediff?.def?.GetModExtension<ArmTextureExtension>();
                if (!seenCurrent && ext?.isArmTexture == true)
                    return true;
            }

            if (node.children != null)
            {
                for (int i = 0; i < node.children.Length; i++)
                {
                    if (HasEarlierArmNodeRecursive(node.children[i], current, ref seenCurrent))
                        return true;
                }
            }

            return false;
        }
    }

    [StaticConstructorOnStartup]
    public static class GunArmOverlayBootstrap
    {
        static GunArmOverlayBootstrap()
        {
            var harmony = new Harmony("Seg.GunArmOverlay");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(RimWorld.PawnOverlayDrawer), "RenderPawnOverlay")]
    public static class Patch_PawnOverlayDrawer_RenderPawnOverlay_GunArm
    {
        private static readonly FieldInfo PawnField =
            typeof(RimWorld.PawnOverlayDrawer).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Postfix(
            RimWorld.PawnOverlayDrawer __instance,
            Matrix4x4 matrix,
            Mesh bodyMesh,
            RimWorld.PawnOverlayDrawer.OverlayLayer layer,
            PawnDrawParms parms,
            bool? overApparel = null)
        {
            if (layer != RimWorld.PawnOverlayDrawer.OverlayLayer.Body)
                return;

            Pawn pawn = PawnField.GetValue(__instance) as Pawn;
            if (pawn == null)
                return;

            if (parms.facing != Rot4.East && parms.facing != Rot4.West)
                return;

            var tree = pawn.Drawer?.renderer?.renderTree;
            if (tree?.rootNode == null)
                return;

            List<PawnRenderNode> arms = CollectArmNodes(tree.rootNode);
            if (arms.Count < 2)
                return;

            PawnRenderNode eastFront = arms[0];
            PawnRenderNode westFront = arms[1];

            PawnRenderNode overlayNode;
            Graphic overlayGraphic;
            bool flip = false;

            if (parms.facing == Rot4.East)
            {
                overlayNode = eastFront;
                overlayGraphic = westFront.PrimaryGraphic;
            }
            else
            {
                overlayNode = westFront;
                overlayGraphic = eastFront.PrimaryGraphic;
                flip = true;
            }
            overlayNode.GetTransform(parms, out Vector3 offset, out Vector3 pivot, out Quaternion rotation, out Vector3 scale);

            if (flip)
            {
                rotation = rotation * Quaternion.Euler(0f, 180f, 0f);
            }

            Vector3 worldPos = pawn.DrawPos + offset;
            worldPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            worldPos.z += 0.1f;

            Matrix4x4 finalMatrix = Matrix4x4.TRS(worldPos, rotation, scale);

            Mesh mesh = overlayNode.GetMesh(parms);

            if (mesh == null)
            {
                PawnRenderNode fallbackNode = (overlayNode == eastFront) ? westFront : eastFront;
                Log.Message($"[GunArmOverlay] mesh null for overlayNode, trying fallback node {fallbackNode?.GetType().Name}");
                if (fallbackNode != null)
                {
                    fallbackNode.GetTransform(parms, out Vector3 fbOffset, out Vector3 fbPivot, out Quaternion fbRotation, out Vector3 fbScale);
                    if (flip)
                        fbRotation = fbRotation * Quaternion.Euler(0f, 0f, 180f);
                    Vector3 fbWorldPos = pawn.DrawPos + fbOffset;
                    fbWorldPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                    fbWorldPos.z += 0.1f;
                    finalMatrix = Matrix4x4.TRS(fbWorldPos, fbRotation, fbScale);
                    mesh = fallbackNode.GetMesh(parms);
                    Log.Message($"[GunArmOverlay] fallback mesh is {(mesh == null ? "null" : "valid")}");
                }
            }

            Material mat = overlayGraphic.MatAt(Rot4.East);

            if (mat == null)
                return;

            if (parms.DrawNow)
            {
                mat.SetPass(0);
                Graphics.DrawMeshNow(mesh, finalMatrix);
            }
            else
            {
                Graphics.DrawMesh(mesh, finalMatrix, mat, 0);
            }
        }

        private static List<PawnRenderNode> CollectArmNodes(PawnRenderNode node)
        {
            List<PawnRenderNode> list = new List<PawnRenderNode>();
            Collect(node, list);
            return list;
        }

        private static void Collect(PawnRenderNode node, List<PawnRenderNode> list)
        {
            if (node is Seg.PawnRenderNode_Arm)
                list.Add(node);

            if (node.children != null)
            {
                for (int i = 0; i < node.children.Length; i++)
                    Collect(node.children[i], list);
            }
        }
    }
}