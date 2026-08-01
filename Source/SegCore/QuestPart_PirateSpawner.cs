using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
namespace seg;


public class GenStep_PirateSpawner : GenStep_Scatterer
{
    public override int SeedPart => 931842770;

    protected override bool CanScatterAt(IntVec3 c, Map map)
    {
        if (base.CanScatterAt(c, map) && c.Standable(map))
        {
            return !c.Fogged(map);
        }
        return false;
    }

    protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
    {
    Pawn pawn;
    var pumps = map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDef.Named("PollutionPump"));
    foreach (var pump in pumps)
    {
        IntVec3 spawnLoc = pump.Position;

        Pawn renegade = PawnGenerator.GeneratePawn(
            DefDatabase<PawnKindDef>.GetNamed("Seg_WOTV_Renegade")
        );

        HealthUtility.DamageUntilDowned(renegade, allowBleedingWounds: false);
        HealthUtility.DamageLegsUntilIncapableOfMoving(renegade, allowBleedingWounds: false);

        GenSpawn.Spawn(renegade, spawnLoc, map);
        renegade.mindState.WillJoinColonyIfRescued = true;
    }
    var monitors = map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDef.Named("VitalsMonitor"));
    foreach (var monitor in monitors)
    {
        IntVec3 spawnLoc = monitor.Position;

        Pawn captain = PawnGenerator.GeneratePawn(
            DefDatabase<PawnKindDef>.GetNamed("Seg_WOTV_PirateCaptain")
        );

        HealthUtility.DamageUntilDowned(captain, allowBleedingWounds: false);
        HealthUtility.DamageLegsUntilIncapableOfMoving(captain, allowBleedingWounds: false);

        GenSpawn.Spawn(captain, spawnLoc, map);
        captain.mindState.WillJoinColonyIfRescued = true;
    }
}
    }
