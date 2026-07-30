using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace seg
{
    [StaticConstructorOnStartup]
    public static class Patch_RemoveServitorChipRemoval
    {
        static Patch_RemoveServitorChipRemoval()
        {
            new Harmony("seg.servitor.removalblock").PatchAll();
        }
    }

    [HarmonyPatch(typeof(HealthCardUtility), "DrawMedOperationsTab")]
    public static class Patch_BlockServitorRemoval_DrawMedOperationsTab
    {
        static readonly HashSet<string> blockedHediffs = new HashSet<string>
        {
            "Seg_Servitors_ServitorizationHediff",
            "Seg_Servitors_MedicaeServitorHediff",
            "Seg_Servitors_CombatServitorHediff",
            "Seg_Servitors_LexomatServitorHediff"
        };

        static readonly HashSet<string> blockedRecipes = new HashSet<string>
        {
            "RemoveImplant_Seg_Servitors_ServitorChip",
            "RemoveImplant_Seg_Servitors_MedicaeChip",
            "RemoveImplant_Seg_Servitors_CombatChip",
            "RemoveImplant_Seg_Servitors_LexomatChip",
            "RemoveHediff_Seg_Servitors_ServitorizationHediff",
            "RemoveHediff_Seg_Servitors_MedicaeServitorHediff",
            "RemoveHediff_Seg_Servitors_CombatServitorHediff",
            "RemoveHediff_Seg_Servitors_LexomatServitorHediff"
        };

        static readonly MethodInfo GenerateSurgeryOptionMI =
            typeof(HealthCardUtility).GetMethod(
                "GenerateSurgeryOption",
                BindingFlags.NonPublic | BindingFlags.Static
            );

        static readonly FieldInfo BillsScrollPositionFI =
            typeof(HealthCardUtility).GetField(
                "billsScrollPosition",
                BindingFlags.NonPublic | BindingFlags.Static
            );

        static readonly FieldInfo BillsScrollHeightFI =
            typeof(HealthCardUtility).GetField(
                "billsScrollHeight",
                BindingFlags.NonPublic | BindingFlags.Static
            );

        public static bool Prefix(
            Rect leftRect,
            Pawn pawn,
            Thing thingForMedBills,
            ref float __result
        )
        {
            float curY = 0f;
            curY += 2f;

            Func<List<FloatMenuOption>> recipeOptionsMaker = () =>
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                int index = 0;

                foreach (RecipeDef recipe in thingForMedBills.def.AllRecipes)
                {
                    if (!recipe.AvailableNow)
                        continue;

                    if (blockedRecipes.Contains(recipe.defName))
                        continue;

                    if (recipe.Worker is Recipe_RemoveBodyPart)
                    {
                        if (pawn.health.hediffSet.hediffs.Any(h => blockedHediffs.Contains(h.def.defName)))
                            continue;
                    }

                    AcceptanceReport report = recipe.Worker.AvailableReport(pawn);
                    if (!report.Accepted && report.Reason.NullOrEmpty())
                        continue;

                    IEnumerable<ThingDef> missing = recipe.PotentiallyMissingIngredients(null, thingForMedBills.MapHeld);
                    if (missing.Any(x => x.isTechHediff))
                        continue;
                    if (missing.Any(x => x.IsDrug))
                        continue;
                    if (missing.Any() && recipe.dontShowIfAnyIngredientMissing)
                        continue;

                    if (recipe.targetsBodyPart)
                    {
                        foreach (BodyPartRecord part in recipe.Worker.GetPartsToApplyOn(pawn, recipe))
                        {
                            if (!recipe.AvailableOnNow(pawn, part))
                                continue;

                            FloatMenuOption opt = (FloatMenuOption)GenerateSurgeryOptionMI.Invoke(
                                null,
                                new object[] { pawn, thingForMedBills, recipe, missing, report, index, part }
                            );

                            list.Add(opt);
                            index++;
                        }
                    }
                    else if (!pawn.health.hediffSet.HasHediff(recipe.addsHediff))
                    {
                        FloatMenuOption opt = (FloatMenuOption)GenerateSurgeryOptionMI.Invoke(
                            null,
                            new object[] { pawn, thingForMedBills, recipe, missing, report, index, null }
                        );

                        list.Add(opt);
                        index++;
                    }
                }

                return list;
            };

            Vector2 scrollPos = (Vector2)BillsScrollPositionFI.GetValue(null);
            float scrollHeight = (float)BillsScrollHeightFI.GetValue(null);

            ((IBillGiver)thingForMedBills).BillStack.DoListing(
                new Rect(leftRect.x - 9f, curY, leftRect.width, leftRect.height - curY - 20f),
                recipeOptionsMaker,
                ref scrollPos,
                ref scrollHeight
            );

            BillsScrollPositionFI.SetValue(null, scrollPos);
            BillsScrollHeightFI.SetValue(null, scrollHeight);

            __result = curY;
            return false;
        }
    }
}