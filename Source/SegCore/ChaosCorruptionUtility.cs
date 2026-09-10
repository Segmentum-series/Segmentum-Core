using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace seg
{
    public class ChaosCorruptionMapComponent : MapComponent
    {
        public ChaosCorruptionGrid Grid;

        public ChaosCorruptionMapComponent(Map map) : base(map)
        {
            Grid = new ChaosCorruptionGrid(map);
        }

        public override void MapComponentTick()
        {
            Grid.CorruptionTick();
        }

        public override void MapComponentUpdate()
        {
            Grid.CorruptionGridUpdate();
        }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref Grid, "ChaosCorruptionGrid", map);
        }
    }

    public class ChaosCorruptionGrid : IExposable
    {
        private BoolGrid grid;
        private Map map;
        private bool dirty;
        private CellBoolDrawer drawer;
        private List<IntVec3> corruptedThisTick = new List<IntVec3>();

        public ChaosCorruptionGrid(Map map)
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

        public bool IsCorrupted(IntVec3 c)
        {
            return c.InBounds(map) && grid[c];
        }

        public bool EverCorruptible(IntVec3 c)
        {
            return c.InBounds(map);
        }

        public bool CanCorrupt(IntVec3 c)
        {
            return EverCorruptible(c) && !grid[c];
        }

        public void SetCorrupted(IntVec3 c, bool val, bool silent = false)
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

        public float TotalCorruptionPercent
        {
            get
            {
                int total = map.AllCells.Count();
                int corrupted = grid.TrueCount;
                return (float)corrupted / (float)total;
            }
        }

        public void CorruptionTick()
        {
            if (dirty)
            {
                Find.WorldGrid[map.Tile].pollution = TotalCorruptionPercent;
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

        public void CorruptionGridUpdate()
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
            return grid[c] ? Color.magenta : Color.white;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref grid, "ChaosCorruptionGridGrid");
        }
    }

    public static class ChaosCorruptionUtility
    {
        private static FastPriorityQueue<IntVec3> tmpQueue;
        private static readonly List<IntVec3> tmpCorrupted = new List<IntVec3>();

        public static void GrowCorruptionAt(
            IntVec3 root,
            Map map,
            int cellsToCorrupt = 6,
            Action<IntVec3> onCorrupt = null,
            bool silent = false)
        {
            if (cellsToCorrupt <= 0) return;

            ChaosCorruptionGrid grid = map.GetComponent<ChaosCorruptionMapComponent>().Grid;

            if (grid.CanCorrupt(root))
            {
                grid.SetCorrupted(root, true, silent);
                onCorrupt?.Invoke(root);
                cellsToCorrupt--;
            }

            if (cellsToCorrupt <= 0) return;

            tmpQueue = new FastPriorityQueue<IntVec3>(
                new ChaosCellComparer(root, map)
            );

            map.floodFiller.FloodFill(
                root,
                c => grid.IsCorrupted(c),
                c => tmpCorrupted.Add(c)
            );

            foreach (IntVec3 c in tmpCorrupted)
            {
                foreach (IntVec3 adj in AdjacentCorruptibleCells(c, map))
                {
                    if (!tmpQueue.Contains(adj))
                        tmpQueue.Push(adj);
                }
            }

            tmpCorrupted.Clear();

            while (cellsToCorrupt > 0 && tmpQueue.Count > 0)
            {
                IntVec3 next = tmpQueue.Pop();

                grid.SetCorrupted(next, true, silent);
                onCorrupt?.Invoke(next);

                foreach (IntVec3 adj in AdjacentCorruptibleCells(next, map))
                {
                    if (!tmpQueue.Contains(adj))
                        tmpQueue.Push(adj);
                }

                cellsToCorrupt--;
            }
        }

        private static IEnumerable<IntVec3> AdjacentCorruptibleCells(IntVec3 c, Map map)
        {
            ChaosCorruptionGrid grid = map.GetComponent<ChaosCorruptionMapComponent>().Grid;

            foreach (IntVec3 dir in GenAdj.CardinalDirections)
            {
                IntVec3 cell = c + dir;
                if (cell.InBounds(map) && grid.CanCorrupt(cell))
                    yield return cell;
            }

            if (Rand.Chance(0.15f))
            {
                foreach (IntVec3 diag in GenAdj.DiagonalDirections)
                {
                    IntVec3 cell = c + diag;
                    if (cell.InBounds(map) && grid.CanCorrupt(cell))
                        yield return cell;
                }
            }
        }

        internal class ChaosCellComparer : IComparer<IntVec3>
        {
            private readonly IntVec3 root;
            private readonly Map map;
            private readonly ModuleBase perlin;

            public ChaosCellComparer(IntVec3 root, Map map)
            {
                this.root = root;
                this.map = map;

                perlin = new Perlin(
                    0.025,
                    2.5,
                    0.6,
                    5,
                    map.uniqueID,
                    QualityMode.Medium
                );

                perlin = new ScaleBias(0.5, 0.5, perlin);
            }

            private float Score(IntVec3 c)
            {
                float dist = Mathf.Max(1f, c.DistanceTo(root));
                float noise = (float)perlin.GetValue(c.x, c.y, c.z);

                float pawnBias = map.mapPawns.AllPawnsSpawned.Any(p => p.Position.DistanceTo(c) <= 6) ? 1.4f : 1f;
                float buildingBias = map.listerBuildings.allBuildingsColonist.Any(b => b.Position.DistanceTo(c) <= 8) ? 1.25f : 1f;

                ChaosCorruptionGrid grid = map.GetComponent<ChaosCorruptionMapComponent>().Grid;
                int adj = 0;
                foreach (IntVec3 a in GenAdj.AdjacentCells)
                {
                    IntVec3 cell = c + a;
                    if (cell.InBounds(map) && grid.IsCorrupted(cell))
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

    public class CompProperties_ChaosCorruptionEmitter : CompProperties
    {
        public int cellsPerPulse = 8;
        public int tickInterval = 300;

        public CompProperties_ChaosCorruptionEmitter()
        {
            compClass = typeof(CompChaosCorruptionEmitter);
        }
    }

    public class CompChaosCorruptionEmitter : ThingComp
    {
        public CompProperties_ChaosCorruptionEmitter Props => (CompProperties_ChaosCorruptionEmitter)props;

        public override void CompTick()
        {
            if (!parent.Spawned) return;
            if (!parent.IsHashIntervalTick(Props.tickInterval)) return;

            Map map = parent.Map;
            ChaosCorruptionMapComponent comp = map.GetComponent<ChaosCorruptionMapComponent>();
            if (comp == null) return;

            ChaosCorruptionUtility.GrowCorruptionAt(
                parent.Position,
                map,
                Props.cellsPerPulse,
                null,
                false
            );
        }
    }

    public static class ChaosCorruptionPawnEffects
    {
        public static void PawnChaosTick(Pawn pawn, int delta)
        {
            if (!pawn.Spawned) return;
            if (!pawn.IsHashIntervalTick(60, delta)) return;

            Map map = pawn.Map;
            ChaosCorruptionMapComponent comp = map.GetComponent<ChaosCorruptionMapComponent>();
            if (comp == null) return;

            if (!comp.Grid.IsCorrupted(pawn.Position)) return;

            if (!pawn.health.hediffSet.HasHediff(HediffDefOf.PollutionStimulus))
                pawn.health.AddHediff(HediffDefOf.PollutionStimulus);
        }
    }

    public static class ChaosWorldCorruption
    {
        public static float TileChaosPercent(int tile)
        {
            return Find.WorldGrid[tile].pollution;
        }

        public static void SetTileChaosPercent(int tile, float val)
        {
            Find.WorldGrid[tile].pollution = Mathf.Clamp01(val);
            Find.World.renderer.Notify_TilePollutionChanged(tile);
        }
    }
}