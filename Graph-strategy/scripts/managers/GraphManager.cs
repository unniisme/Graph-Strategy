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
        Logger logger = new("GraphManager"); 

        private NavigationGridGraph gridGraph = null;
        private DualGridGraph gridGraphDual = null;
        private IGrid grid = null;
        private IslandBridgeGraph<Vector2I> islandGraph = null;
        private DualIslandBridgeGraph islandGraphDual = null;
        private IShannonStrategy<Vector2I> shannonStrategyShort;
        private IShannonStrategy<Vector2I> shannonStrategyCut;

        bool draw = false;
        bool drawDual = false;

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

        public void DebugDual()
        {
            drawDual = !drawDual;
            QueueRedraw();
        }

        public override void _Ready()
        {
            cutPhantom = GameManager.GetLevel().phantoms[0];
            cutPhantom.Placed += MakeCutMove;

            shortPhantom = GameManager.GetLevel().phantoms[1];
            shortPhantom.Placed += MakeShortMove;

            CalculateIslands();
        }

        public void CalculateIslands()
        {
            grid ??= GameManager.GetLevel().grid;
            Vector2I pos = grid.GameCoordinateToGridCoordinate(shortSpot.Position);

            gridGraph = new(grid, pos)
            {
                Trace = new("gridGraph")
            };
            islandGraph = new IslandBridgeGraph<Vector2I>(gridGraph.DataNodeMap[pos], new("islandGraph"));

            gridGraphDual = new(grid, islandGraph)
            {
                Trace = new("gridGraphDual")
            };
            Vector2I dualPos = gridGraphDual.DataNodeMap.Keys.First();
            islandGraphDual = new DualIslandBridgeGraph(gridGraphDual.DataNodeMap[dualPos], islandGraph, new("islandGraphDual"));


            shannonStrategyShort = new GraphUpdateShannonStrategy<Vector2I>(islandGraph)
            {
                Trace = new("shannonStrategyShort")
            };
            shannonStrategyCut = new GraphUpdateShannonStrategy<Vector2I>(islandGraphDual)
            {
                Trace = new("shannonStrategyCut")
            };

            shannonStrategyShort.FindSpanningTrees();
            shannonStrategyCut.FindSpanningTrees();

            if (debugWrite) 
                GD.Print(ShowIslandGraph(islandGraph));
        }

        private void MakeShortMove(Vector2I pos)
        {
            if (!islandGraph.islets.ContainsElement(pos)) return;
            Vector2I moveBridge = islandGraph.islets.Find(pos);
            shannonStrategyShort.Short(moveBridge);
            shannonStrategyCut.Cut(moveBridge);

            Vector2I cutMove = shannonStrategyCut.GetShortMove();
            GD.Print($"Cut Move : {cutMove}");

            QueueRedraw();
        }

        private void MakeCutMove(Vector2I pos)
        {
            if (!islandGraph.islets.ContainsElement(pos)) return;
            Vector2I moveBridge = islandGraph.islets.Find(pos);
            shannonStrategyShort.Cut(moveBridge);
            shannonStrategyCut.Short(moveBridge);

            Vector2I shortMove = shannonStrategyShort.GetShortMove();
            GD.Print($"Short Move : {shortMove}");

            QueueRedraw();
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
                if (drawDual)
                {
                    if (debugDraw)
                    {
                        DrawGraph(gridGraphDual, 4, Colors.Blue);
                        DrawMultiGraph(islandGraphDual, 7, Colors.Red);
                    }

                    // if (debugDrawSpanningTrees)
                    // {
                    //     DrawEdges(spanningTreesDual.Item1, 10,  Colors.GreenYellow);
                    //     DrawEdges(spanningTreesDual.Item2, 10,  Colors.BlanchedAlmond);
                    // }
                }
                else
                {
                    if (debugDraw)
                    {
                        DrawGraph(gridGraph, 4, Colors.Blue);
                        DrawMultiGraph(islandGraph, 7, Colors.Red);
                    }

                    // if (debugDrawSpanningTrees)
                    // {
                    //     DrawEdges(spanningTrees.Item1, 10,  Colors.GreenYellow);
                    //     DrawEdges(spanningTrees.Item2, 10,  Colors.BlanchedAlmond);
                    // }
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