using System;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Grid;
using Gamelogic.Grid.Graph;
using Godot;
using Graphs;
using Graphs.Shannon;
using Logging;

namespace Gamelogic.Managers
{
    [GlobalClass]
    public partial class GraphManager : Node2D
    {
        private NavigationGridGraph gridGraph = null;
        private DualGridGraph gridGraphDual = null;
        private IGrid grid = null;
        private Graph<Islet<Vector2I>> islandGraph = null;
        private Graph<Islet<Vector2I>> islandGraphDual = null;
        private Tuple<List<Edge<Islet<Vector2I>>>, List<Edge<Islet<Vector2I>>>> spanningTrees = null;
        private Tuple<List<Edge<Islet<Vector2I>>>, List<Edge<Islet<Vector2I>>>> spanningTreesDual = null;
        private IShannonStrategy<Islet<Vector2I>> shannonStrategyShort;
        private IShannonStrategy<Islet<Vector2I>> shannonStrategyCut;

        bool draw = false;

        [Export]
        public bool debugWrite = false;
        [Export]
        public bool debugDraw = false;
        [Export]
        public bool debugDrawSpanningTrees = true;

        [Export]
        public Node2D shortSpot;
        [Export]

        public Node2D cutSpot;

        BuildPhantom cutPhantom; // blue
        BuildPhantom shortPhantom; // red

        public void Debug()
        {
            if (grid == null)
            {
                CalculateIslands();
            }

            draw = !draw;
            QueueRedraw();
        }

        public static string IsletToString(Islet<Vector2I> graph)
        {
            if (graph == null) return "null";

            string isIsland = graph.IsIsland?"Island":"Bridge";
            List<Vector2I> nodes = IsletToGridPositions(graph);
            return $"[{graph.UID}] {isIsland} {{ {string.Join(",", nodes)} }}";
        }

        private static List<Vector2I> IsletToGridPositions(Islet<Vector2I> islet)
        {
            List<Vector2I> nodes = new();
            foreach (Vector2I node in islet)
            {
                nodes.Add(node);
            }
            return nodes;
        }

        public override void _Ready()
        {
            cutPhantom = GameManager.GetLevel().phantoms[0];
            cutPhantom.TurnedOff += MakeCutMove;

            shortPhantom = GameManager.GetLevel().phantoms[1];
            shortPhantom.TurnedOff += MakeShortMove;

            CalculateIslands();
        }

        public void CalculateIslands()
        {
            grid ??= GameManager.GetLevel().grid;
            Vector2I pos = grid.GameCoordinateToGridCoordinate(shortSpot.Position);

            gridGraph = new(grid, pos);
            islandGraph = IslandBridgeAlgorithm<Vector2I>.GetIslandBridgeGraph(gridGraph.DataNodeMap[pos]);

            gridGraphDual = new(grid, islandGraph);
            Vector2I dualPos = gridGraphDual.DataNodeMap.Keys.First();
            islandGraphDual = DualIslandBridgeAlgorithm.GetIslandBridgeGraph(gridGraphDual.DataNodeMap[dualPos], islandGraph);

            foreach (Islet<Vector2I> islet in islandGraphDual.DataNodeMap.Keys)
            {
                Logger.Debug(IsletToString(islet));
            }


            shannonStrategyShort = new TwoPlayerShannonStrategy<Islet<Vector2I>>(islandGraph)
            {
                DataToString = IsletToString
            };
            shannonStrategyCut = new TwoPlayerShannonStrategy<Islet<Vector2I>>(islandGraphDual)
            {
                DataToString = IsletToString
            };

            Vector2I shortGridSpot = grid.GameCoordinateToGridCoordinate(shortSpot.Position);
            Vector2I cutGridSpot = grid.GameCoordinateToGridCoordinate(cutSpot.Position);

            spanningTrees = shannonStrategyShort.GetSpanningTrees(FindIslet(islandGraph, shortGridSpot), FindIslet(islandGraph, cutGridSpot));
            spanningTreesDual = shannonStrategyCut.GetSpanningTrees(null, null);

            if (debugWrite) 
                GD.Print(ShowIslandGraph(islandGraph));
        }

        private void MakeShortMove()
        {
            Vector2I pos = shortPhantom.GridPosition;
            Islet<Vector2I> moveBridge = FindIslet(islandGraph, pos, false);
            if (moveBridge == null) return;
            shannonStrategyShort.Short(moveBridge);
            shannonStrategyCut.Cut(moveBridge);

            Islet<Vector2I> cutMove = shannonStrategyCut.GetShortMove();
            GD.Print($"Cut Move : {IsletToString(cutMove)}");
        }

        private void MakeCutMove()
        {
            Vector2I pos = cutPhantom.GridPosition;
            Islet<Vector2I> moveBridge = FindIslet(islandGraph, pos, false);
            if (moveBridge == null) return;
            shannonStrategyShort.Cut(moveBridge);
            shannonStrategyCut.Short(moveBridge);

            Islet<Vector2I> shortMove = shannonStrategyShort.GetShortMove();
            GD.Print($"Short Move : {IsletToString(shortMove)}");
        }

        private Islet<Vector2I> FindIslet(Graph<Islet<Vector2I>> islandGraph, Vector2I gridPos, bool isIsland = true)
        {
            if (isIsland)
            {
                foreach (Node<Islet<Vector2I>> island in islandGraph.Nodes)
                {
                    if (IsletToGridPositions(island.Data).Contains(gridPos))
                    {
                        return island.Data;
                    }
                }
            }
            else
            {
                foreach (Edge<Islet<Vector2I>> bridge in islandGraph.Edges)
                {
                    if (IsletToGridPositions(bridge.Data).Contains(gridPos))
                    {
                        return bridge.Data;
                    }
                }
            }
            return null;
        }

        private void DrawIslands()
        {
            foreach (Islet<Vector2I> islet in islandGraphDual.DataNodeMap.Keys)
            {
                DrawIslet(islet, 5, Colors.Green);
                
            }
            foreach (Edge<Islet<Vector2I>> edge in islandGraphDual.Edges)
            {
                DrawIslet(edge.Data, 3, Colors.Red);
            }
        }

        private void DrawIslet(Islet<Vector2I> islet, float radius, Color col)
        {
            foreach (Vector2I pos in islet)
            {
                DrawCircle(grid.GridCoordinateToGameCoordinate(pos), radius, col);
            }
        }

        private void DrawEdges(List<Edge<Islet<Vector2I>>> edges, float radius, Color color)
        {
            foreach (Edge<Islet<Vector2I>> edge in edges)
            {
                DrawIslet(edge.Data, radius, color);
            }
        }

        private void DrawGraph(Graph<Vector2I> graph, float radius, Color col)
        {
            foreach (Node<Vector2I> node in graph.Nodes)
            {
                Vector2I pos = node.Data;
                DrawCircle(grid.GridCoordinateToGameCoordinate(pos), radius, col);
            }
            foreach (Edge<Vector2I> edge in graph.Edges)
            {
                Vector2 from = grid.GridCoordinateToGameCoordinate(edge.FromNode.Data);
                Vector2 to = grid.GridCoordinateToGameCoordinate(edge.ToNode.Data);

                DrawLine(from, to, col);
            }
        }

        public override void _Draw()
        {
            if (gridGraph == null) return;

            if (draw)
            {
                if (debugDraw)
                {
                    DrawGraph(gridGraphDual, 4, Colors.Blue);
                    DrawIslands();
                }

                if (debugDrawSpanningTrees)
                {
                    DrawEdges(spanningTreesDual.Item1, 10,  Colors.GreenYellow);
                    DrawEdges(spanningTreesDual.Item2, 10,  Colors.BlanchedAlmond);
                }
            }

        }

        private string ShowIslandGraph(Graph<Islet<Vector2I>> graph)
        {
            string outString = "";
            foreach (Node<Islet<Vector2I>> node in graph.Nodes)
            {
                string children = "";
                foreach (Edge<Islet<Vector2I>> edge in node.AdjList)
                {
                    children = $"{children}\n\t{ShowIslet(edge.Data)}\n\t\t{ShowIslet(edge.ToNode.Data)}";
                }
                outString = $"{outString}\n{ShowIslet(node.Data)}{children}";
            }
            return outString;
        }

        private string ShowIslet(Islet<Vector2I> islet)
        {
            string isletString = islet.IsIsland?"Island :":"Bridge :";
            foreach (Vector2I pos in islet)
            {
                isletString = $"{isletString} {pos}";
            }
            return isletString;
        }
    }
}