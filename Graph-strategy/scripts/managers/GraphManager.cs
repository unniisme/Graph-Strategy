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
        // private DualGridGraph gridGraphDual = null;
        private IGrid grid = null;
        private IslandBridgeGraph<Vector2I> islandGraph = null;
        // private IslandBridgeGraph<Vector2I> islandGraphDual = null;
        private Tuple<List<Edge<Vector2I>>, List<Edge<Vector2I>>> spanningTrees = null;
        // private Tuple<List<Edge<Vector2I>>, List<Edge<Vector2I>>> spanningTreesDual = null;
        private IShannonStrategy<Vector2I> shannonStrategyShort;
        // private IShannonStrategy<Vector2I> shannonStrategyCut;

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
            islandGraph = new IslandBridgeGraph<Vector2I>(gridGraph.DataNodeMap[pos]);

            // gridGraphDual = new(grid, islandGraph);
            // Vector2I dualPos = gridGraphDual.DataNodeMap.Keys.First();
            // islandGraphDual = DualIslandBridgeAlgorithm.GetIslandBridgeGraph(gridGraphDual.DataNodeMap[dualPos], islandGraph);


            shannonStrategyShort = new TwoPlayerShannonStrategy<Vector2I>(islandGraph);
            // shannonStrategyCut = new TwoPlayerShannonStrategy<Vector2I>(islandGraphDual);

            Vector2I shortGridSpot = grid.GameCoordinateToGridCoordinate(shortSpot.Position);
            Vector2I cutGridSpot = grid.GameCoordinateToGridCoordinate(cutSpot.Position);

            spanningTrees = shannonStrategyShort.GetSpanningTrees(islandGraph.islets.Find(shortGridSpot), islandGraph.islets.Find(cutGridSpot));
            // spanningTreesDual = shannonStrategyCut.GetSpanningTrees(null, null);

            if (debugWrite) 
                GD.Print(ShowIslandGraph(islandGraph));
        }

        private void MakeShortMove()
        {
            Vector2I pos = shortPhantom.GridPosition;
            if (!islandGraph.islets.ContainsElement(pos)) return;
            Vector2I moveBridge = islandGraph.islets.Find(pos);
            shannonStrategyShort.Short(moveBridge);
            // shannonStrategyCut.Cut(moveBridge);

            // Vector2I cutMove = shannonStrategyCut.GetShortMove();
        }

        private void MakeCutMove()
        {
            Vector2I pos = cutPhantom.GridPosition;
            if (!islandGraph.islets.ContainsElement(pos)) return;
            Vector2I moveBridge = islandGraph.islets.Find(pos);
            shannonStrategyShort.Cut(moveBridge);
            // shannonStrategyCut.Short(moveBridge);

            Vector2I shortMove = shannonStrategyShort.GetShortMove();
            GD.Print($"Short Move : {shortMove}");
        }

        private void DrawEdges(List<Edge<Vector2I>> edges, float radius, Color color)
        {
            foreach (Edge<Vector2I> edge in edges)
            {
                DrawCircle(grid.GridCoordinateToGameCoordinate(edge.Data), radius, color);
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

        private void DrawMultiGraph(Graph<Vector2I> graph, float radius, Color col)
        {
            foreach (Node<Vector2I> node in graph.Nodes)
            {
                Vector2I pos = node.Data;
                DrawCircle(grid.GridCoordinateToGameCoordinate(pos), radius, col);
            }
            foreach (Edge<Vector2I> edge in graph.Edges)
            {
                Vector2 from = grid.GridCoordinateToGameCoordinate(edge.FromNode.Data);
                Vector2 edgePos = grid.GridCoordinateToGameCoordinate(edge.Data);
                Vector2 to = grid.GridCoordinateToGameCoordinate(edge.ToNode.Data);

                DrawLine(from, edgePos, col);
                DrawLine(edgePos, to, col);
            }
        }

        public override void _Draw()
        {
            if (gridGraph == null) return;

            if (draw)
            {
                if (debugDraw)
                {
                    DrawGraph(gridGraph, 4, Colors.Blue);
                    DrawMultiGraph(islandGraph, 7, Colors.Red);
                }

                if (debugDrawSpanningTrees)
                {
                    DrawEdges(spanningTrees.Item1, 10,  Colors.GreenYellow);
                    DrawEdges(spanningTrees.Item2, 10,  Colors.BlanchedAlmond);
                }
            }

        }

        private static string ShowIslandGraph(IslandBridgeGraph<Vector2I> graph)
        {
            string outString = "";
            foreach (Node<Vector2I> node in graph.Nodes)
            {
                string children = "";
                foreach (Edge<Vector2I> edge in node.AdjList)
                {
                    children = $"{children}\n\t{edge.Data}\n\t\t{edge.ToNode.Data}";
                }
                outString = $"{outString}\n{node.Data}{children}";
            }
            return outString;
        }
    }
}