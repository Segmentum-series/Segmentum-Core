using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace seg
{
    public class NurgleCorruptionMapComponent : MapComponent
        {
            public NurgleCorruptionGrid Grid;
            public ModuleBase NurglePerlin;

            public NurgleCorruptionMapComponent(Map map) : base(map)
            {
                Grid = new NurgleCorruptionGrid(map);

                NurglePerlin = new ScaleBias(
                    0.5,
                    0.5,
                    new Perlin(
                        0.025,
                        2.5,
                        0.6,
                        5,
                        map.uniqueID,
                        QualityMode.Medium
                    )
                );
            }

            public override void MapComponentTick()
            {
                Grid.NurgleCorruptionTick();
            }

            public override void MapComponentUpdate()
            {
                Grid.NurgleCorruptionGridUpdate();
            }

            public override void ExposeData()
            {
                Scribe_Deep.Look(ref Grid, "NurgleCorruptionGrid", map);
            }
        }
    public class NurgleCorruptionGrid : IExposable
    {
        private BoolGrid grid;
        private Map map;
        private bool dirty;
        private CellBoolDrawer drawer;
        private List<IntVec3> corruptedThisTick = new List<IntVec3>();

        public NurgleCorruptionGrid(Map map)
        {
            this.map = map;
            grid = new BoolGrid(map);
            drawer = new CellBoolDrawer(
                i => CellBool(i),
                () => Color.white,
                i => ExtraColor(i),
                map.Size.x,
                map.Size.z
            );
        }

        public bool IsNurgleCorrupted(IntVec3 c)
        {
            return c.InBounds(map) && grid[c];
        }

        public bool EverNurgleCorruptible(IntVec3 c)
        {
            return c.InBounds(map);
        }

        public bool CanNurgleCorrupt(IntVec3 c)
        {
            return EverNurgleCorruptible(c) && !grid[c];
        }

        public void SetNurgleCorrupted(IntVec3 c, bool val, bool silent = false)
        {
            if (!c.InBounds(map)) return;
            if (grid[c] == val) return;
            grid.Set(c, val);
            dirty = true;
            map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Terrain);
            map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Buildings);
            drawer.SetDirty();
            if (!silent && val) corruptedThisTick.Add(c);
        }

        public float TotalNurgleCorruptionPercent
        {
            get
            {
                int total = map.AllCells.Count();
                int corrupted = grid.TrueCount;
                return (float)corrupted / (float)total;
            }
        }

        public void NurgleCorruptionTick()
        {
            if (dirty)
            {
                Find.WorldGrid[map.Tile].pollution = TotalNurgleCorruptionPercent;
                Find.World.renderer.Notify_TilePollutionChanged(map.Tile);
                dirty = false;
            }

            if (corruptedThisTick.Count > 0)
            {
                EffecterDef fx = EffecterDefOf.CellPollution;
                foreach (IntVec3 c in corruptedThisTick)
                {
                    map.effecterMaintainer.AddEffecterToMaintain(
                        fx.Spawn(c, map, Vector3.zero),
                        c,
                        45
                    );
                }
            }

            corruptedThisTick.Clear();
        }

        public void NurgleCorruptionGridUpdate()
        {
            if (Find.PlaySettings.showPollutionOverlay && !Find.ScreenshotModeHandler.Active)
                drawer.MarkForDraw();
            drawer.CellBoolDrawerUpdate();
        }

        private bool CellBool(int index)
        {
            IntVec3 c = CellIndicesUtility.IndexToCell(index, map.Size.x);
            return c.InBounds(map) && !c.Fogged(map) && grid[c];
        }

        private Color ExtraColor(int index)
        {
            IntVec3 c = CellIndicesUtility.IndexToCell(index, map.Size.x);
            if (grid[c])
                return new Color(0.15f, 0f, 0.25f, 0.65f);
            return new Color(0f, 0f, 0f, 0f);
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref grid, "NurgleCorruptionGridGrid");
        }
    }

    public static class NurgleCorruptionUtility
    {
        private static FastPriorityQueue<IntVec3> tmpQueue;
        private static readonly List<IntVec3> tmpCorrupted = new List<IntVec3>();

        public static void GrowNurgleCorruptionAt(
            IntVec3 root,
            Map map,
            int cellsToCorrupt = 6,
            Action<IntVec3> onCorrupt = null,
            bool silent = false)
        {
            if (cellsToCorrupt <= 0) return;

            NurgleCorruptionGrid grid = map.GetComponent<NurgleCorruptionMapComponent>().Grid;

            if (grid.CanNurgleCorrupt(root))
            {
                grid.SetNurgleCorrupted(root, true, silent);
                onCorrupt?.Invoke(root);
                cellsToCorrupt--;
            }

            if (cellsToCorrupt <= 0) return;

            tmpQueue = new FastPriorityQueue<IntVec3>(
                new NurgleCellComparer(root, map)
            );

            map.floodFiller.FloodFill(
                root,
                c => grid.IsNurgleCorrupted(c),
                c => tmpCorrupted.Add(c)
            );

            foreach (IntVec3 c in tmpCorrupted)
            {
                foreach (IntVec3 adj in AdjacentNurgleCorruptibleCells(c, map))
                {
                    if (!tmpQueue.Contains(adj))
                        tmpQueue.Push(adj);
                }
            }

            tmpCorrupted.Clear();

            while (cellsToCorrupt > 0 && tmpQueue.Count > 0)
            {
                IntVec3 next = tmpQueue.Pop();

                grid.SetNurgleCorrupted(next, true, silent);
                onCorrupt?.Invoke(next);

                foreach (IntVec3 adj in AdjacentNurgleCorruptibleCells(next, map))
                {
                    if (!tmpQueue.Contains(adj))
                        tmpQueue.Push(adj);
                }

                cellsToCorrupt--;
            }
        }
        private static IEnumerable<IntVec3> AdjacentNurgleCorruptibleCells(IntVec3 c, Map map)
        {
            NurgleCorruptionGrid grid = map.GetComponent<NurgleCorruptionMapComponent>().Grid;

            foreach (IntVec3 dir in GenAdj.CardinalDirections)
            {
                IntVec3 cell = c + dir;
                if (cell.InBounds(map) && grid.CanNurgleCorrupt(cell))
                    yield return cell;
            }

            if (Rand.Chance(0.15f))
            {
                foreach (IntVec3 diag in GenAdj.DiagonalDirections)
                {
                    IntVec3 cell = c + diag;
                    if (cell.InBounds(map) && grid.CanNurgleCorrupt(cell))
                        yield return cell;
                }
            }
        }

        internal class NurgleCellComparer : IComparer<IntVec3>
            {
                private readonly IntVec3 root;
                private readonly Map map;
                private readonly ModuleBase perlin;

                public NurgleCellComparer(IntVec3 root, Map map)
                {
                    this.root = root;
                    this.map = map;
                    perlin = map.GetComponent<NurgleCorruptionMapComponent>().NurglePerlin;
                }

                private float Score(IntVec3 c)
                {
                    float dist = Mathf.Max(1f, c.DistanceTo(root));
                    float noise = (float)perlin.GetValue(c.x, c.y, c.z);

                    float pawnBias = map.mapPawns.AllPawnsSpawned.Any(p => p.Position.DistanceTo(c) <= 6) ? 1.4f : 1f;
                    float buildingBias = map.listerBuildings.allBuildingsColonist.Any(b => b.Position.DistanceTo(c) <= 8) ? 1.25f : 1f;

                    NurgleCorruptionGrid grid = map.GetComponent<NurgleCorruptionMapComponent>().Grid;
                    int adj = 0;
                    foreach (IntVec3 a in GenAdj.AdjacentCells)
                    {
                        IntVec3 cell = c + a;
                        if (cell.InBounds(map) && grid.IsNurgleCorrupted(cell))
                            adj++;
                    }

                    return
                        (1f / dist) *
                        (1f + noise * 2f) *
                        (1f + adj * 0.3f) *
                        pawnBias *
                        buildingBias;
                }

                public int Compare(IntVec3 a, IntVec3 b)
                {
                    float sa = Score(a);
                    float sb = Score(b);
                    if (sa < sb) return 1;
                    if (sa > sb) return -1;
                    return 0;
                }
            }
    }

    public static class NurgleConversion
    {
        private static readonly Dictionary<TerrainDef, TerrainDef> convertMap = new Dictionary<TerrainDef, TerrainDef>();
        private static readonly Dictionary<TerrainDef, TerrainDef> reverseMap = new Dictionary<TerrainDef, TerrainDef>();

        public static void Initialize()
        {
            AddPair("SoilRich", "Seg_NurgleSoil");
            AddPair("Soil", "Seg_NurgleSoil");
            AddPair("Gravel", "Seg_NurgleSoil");
            AddPair("Mud", "Seg_NurgleSoil");
            AddPair("RiverBank", "Seg_NurgleSoil");
            AddPair("WaterDeep", "Seg_NurgleWater");
            AddPair("WaterShallow", "Seg_NurgleWater");
            AddPair("Marsh", "Seg_NurgleSoil");
            AddPair("Sand", "Seg_NurgleSand");
            AddPair("Ice", "Seg_NurgleIce");
        }

        private static void AddPair(string original, string chaos)
        {
            TerrainDef o = DefDatabase<TerrainDef>.GetNamed(original, false);
            TerrainDef c = DefDatabase<TerrainDef>.GetNamed(chaos, false);
            if (o != null && c != null)
            {
                convertMap[o] = c;
                reverseMap[c] = o;
            }
        }

        public static void ConvertNurgleCell(IntVec3 c, Map map)
        {
            TerrainDef t = map.terrainGrid.TerrainAt(c);
            if (convertMap.TryGetValue(t, out TerrainDef chaos))
                map.terrainGrid.SetTerrain(c, chaos);
        }

        public static void ReverseNurgleCell(IntVec3 c, Map map)
        {
            TerrainDef t = map.terrainGrid.TerrainAt(c);
            if (reverseMap.TryGetValue(t, out TerrainDef clean))
                map.terrainGrid.SetTerrain(c, clean);
        }

        public static void BloomNurgleConvert(IntVec3 root, Map map, int count)
        {
            NurgleCorruptionUtility.GrowNurgleCorruptionAt(
                root,
                map,
                count,
                c => ConvertNurgleCell(c, map),
                true
            );
        }

        public static void BloomNurgleReverse(IntVec3 root, Map map, int count)
        {
            NurgleCorruptionUtility.GrowNurgleCorruptionAt(
                root,
                map,
                count,
                c => ReverseNurgleCell(c, map),
                true
            );
        }
    }

    public class CompProperties_NurgleConversionEmitter : CompProperties
    {
        public int cellsPerPulse = 6;
        public int tickInterval = 300;
        public int radius = 8;

        public CompProperties_NurgleConversionEmitter()
        {
            compClass = typeof(CompNurgleConversionEmitter);
        }
    }

    public class CompNurgleConversionEmitter : ThingComp
        {
            public CompProperties_NurgleConversionEmitter Props => (CompProperties_NurgleConversionEmitter)props;

            public override void CompTick()
            {
                if (!parent.Spawned) return;
                if (!parent.IsHashIntervalTick(Props.tickInterval)) return;
                NurgleConversion.Initialize();
                NurgleConversion.BloomNurgleConvert(parent.Position, parent.Map, Props.cellsPerPulse);
            }
        }

    public class CompProperties_NurgleReversalEmitter : CompProperties
    {
        public int cellsPerPulse = 6;
        public int tickInterval = 300;
        public int radius = 8;

        public CompProperties_NurgleReversalEmitter()
        {
            compClass = typeof(CompNurgleReversalEmitter);
        }
    }

    public class CompNurgleReversalEmitter : ThingComp
    {
        public CompProperties_NurgleReversalEmitter Props => (CompProperties_NurgleReversalEmitter)props;

        public override void CompTick()
        {
            if (!parent.Spawned) return;
            if (!parent.IsHashIntervalTick(Props.tickInterval)) return;

            NurgleConversion.Initialize();

            foreach (IntVec3 c in GenRadial.RadialCellsAround(parent.Position, Props.radius, true))
            {
                NurgleConversion.BloomNurgleReverse(c, parent.Map, Props.cellsPerPulse);
            }
        }
    }

    public static class NurgleCorruptionPawnEffects
    {
        public static void PawnNurgleTick(Pawn pawn, int delta)
        {
            if (!pawn.Spawned) return;
            if (!pawn.IsHashIntervalTick(60, delta)) return;

            Map map = pawn.Map;
            NurgleCorruptionMapComponent comp = map.GetComponent<NurgleCorruptionMapComponent>();
            if (comp == null) return;

            if (!comp.Grid.IsNurgleCorrupted(pawn.Position)) return;

            if (!pawn.health.hediffSet.HasHediff(HediffDefOf.PollutionStimulus))
                pawn.health.AddHediff(HediffDefOf.PollutionStimulus);
        }
    }

    public static class NurgleWorldCorruption
    {
        public static float TileNurglePercent(int tile)
        {
            return Find.WorldGrid[tile].pollution;
        }

        public static void SetTileNurglePercent(int tile, float val)
        {
            Find.WorldGrid[tile].pollution = Mathf.Clamp01(val);
            Find.World.renderer.Notify_TilePollutionChanged(tile);
        }
    }
}