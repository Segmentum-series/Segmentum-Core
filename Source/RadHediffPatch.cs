using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;

namespace Segmentum.Core
{

public class SEG_Core_Rad_Verb_Shoot : Verb_Shoot
    {
        private static readonly HediffDef RadBuildupDef = HediffDef.Named("SEG_RadBuildup");

        protected override bool TryCastShot()
        {
            bool fired = base.TryCastShot();
            if (!fired)
                return false;

            if (!CasterIsPawn)
                return true;

            Pawn pawn = CasterPawn;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(RadBuildupDef);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(RadBuildupDef, pawn);
                pawn.health.AddHediff(hediff);
            }

            hediff.Severity += 0.001f;

            return true;
        }
    }
}