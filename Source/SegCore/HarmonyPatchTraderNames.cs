using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace seg
{
    [HarmonyPatch(typeof(TradeShip))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new System.Type[] { typeof(TraderKindDef), typeof(Faction) })]
    public static class Patch_TradeShip_Namer
    {
        static void Postfix(TradeShip __instance, TraderKindDef def, Faction faction)
        {
            
            if (def.defName == "Seg_COTO_Orbital_Mechanicus")
            {
                List<string> existingNames = new List<string>();
                foreach (Map map in Find.Maps)
                {
                    foreach (PassingShip ship in map.passingShipManager.passingShips)
                    {
                        if (!string.IsNullOrEmpty(ship.name))
                            existingNames.Add(ship.name);
                    }
                }
                string newName = NameGenerator.GenerateName(
                    DefDatabase<RulePackDef>.GetNamed("Seg_COTO_NamerTraderMechanicus"),
                    existingNames
                );

                // vanilla code had this, 
                if (faction != null)
                {
                    newName = "GuildTradeShipName".Translate(newName, faction.Name);
                }

                __instance.name = newName;
            }
        }
    }
}