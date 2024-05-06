using System;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Grid;
using Gamelogic.Grid.Graph;
using Godot;
using Graphs;
using Graphs.Shannon;
using Graphs.Utils;
using Logging;
using Utils;

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
        private IShannonStrategy<WeightedData<Vector2I>> shannonStrategyShort;
        private IShannonStrategy<WeightedData<Vector2I>> shannonStrategyCut;

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

        [Export]
        public Node2D outerSpot;
        [Export]
        public Node2D innerSpot;

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

            Vector2I outer = grid.GameCoordinateToGridCoordinate(outerSpot.Position);
            Vector2I inner = grid.GameCoordinateToGridCoordinate(innerSpot.Position);
            islandGraphDual = new DualIslandBridgeGraph(gridGraphDual.DataNodeMap[outer], gridGraphDual.DataNodeMap[inner],
                                                            islandGraph, new("islandGraphDual", LogLevel.DEBUG)
                                                        );



            Vector2I source = islandGraph.islets.Find(GridPosition(cutSpot)); 
            Vector2I sink = islandGraph.islets.Find(GridPosition(shortSpot)); 
            shannonStrategyShort = new WeightedShannonStrategy<Vector2I>(islandGraph, source, sink)
            {
                Trace = new("shannonStrategyShort")
            };
            shannonStrategyCut = new WeightedShannonStrategy<Vector2I>(islandGraphDual, islandGraphDual.UpperIslet, islandGraphDual.LowerIslet)
            {
                Trace = new("shannonStrategyCut")
            };

            shannonStrategyShort.FindSpanningTrees();
            shannonStrategyCut.FindSpanningTrees();

            if (GameManager.GetLevel().currentTeam == 0)
            {
                var cutMove = shannonStrategyCut.ShortMove;
                GD.Print($"Cut Move : {cutMove}");
            }
            else
            {
                var shortMove = shannonStrategyShort.ShortMove;
                GD.Print($"Short Move : {shortMove}");
            }

            if (debugWrite) 
                GD.Print(ShowIslandGraph(islandGraph));
        }

        private void MakeShortMove(Vector2I pos)
        {
            if (islandGraph.islets.ContainsElement(pos))
            {
                Vector2I moveBridge = islandGraph.islets.Find(pos);
                shannonStrategyShort.Short(WeightedData<Vector2I>.Edge(moveBridge,1));
                shannonStrategyShort.Short(WeightedData<Vector2I>.Edge(moveBridge,2));
                shannonStrategyShort.Clear();
                shannonStrategyShort.FindSpanningTrees();
            }
            if (islandGraphDual.islets.ContainsElement(pos))
            {
                Vector2I moveBridge = islandGraphDual.islets.Find(pos);
                shannonStrategyCut.Cut(WeightedData<Vector2I>.Edge(moveBridge, 1));
                shannonStrategyCut.Cut(WeightedData<Vector2I>.Edge(moveBridge, 2));
                shannonStrategyCut.Clear();
                shannonStrategyCut.FindSpanningTrees();
            }
            QueueRedraw();

            var cutMove = shannonStrategyCut.ShortMove;
            GD.Print($"Cut Move : {cutMove}");
        }

        private void MakeCutMove(Vector2I pos)
        {
            if (islandGraph.islets.ContainsElement(pos))
            {
                Vector2I moveBridge = islandGraph.islets.Find(pos);
                shannonStrategyShort.Cut(WeightedData<Vector2I>.Edge(moveBridge,1));
                shannonStrategyShort.Cut(WeightedData<Vector2I>.Edge(moveBridge,2));
                shannonStrategyShort.Clear();
                shannonStrategyShort.FindSpanningTrees();

                
            }
            if (islandGraphDual.islets.ContainsElement(pos))
            {
                Vector2I moveBridge = islandGraphDual.islets.Find(pos);
                shannonStrategyCut.Short(WeightedData<Vector2I>.Edge(moveBridge, 1));
                shannonStrategyCut.Short(WeightedData<Vector2I>.Edge(moveBridge, 2));
                shannonStrategyCut.Clear();
                shannonStrategyCut.FindSpanningTrees();
            }
            QueueRedraw();

            var shortMove = shannonStrategyShort.ShortMove;
            GD.Print($"Short Move : {shortMove}");
        }

        private Vector2I GridPosition(Node2D node) => grid.GameCoordinateToGridCoordinate(node.Position);


        //------------------Rendering----------------------------------------------------

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

        private void DrawSpanningTrees(SpanningTree<WeightedData<Vector2I>> spanningTree, float radius, Color col)
        {
            foreach (Edge<WeightedData<Vector2I>> edge in spanningTree.Edges)
            {
                DrawCircle(grid.GridCoordinateToGameCoordinate(edge.Data.item), radius, col);
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

                    if (debugDrawSpanningTrees)
                    {
                        DrawSpanningTrees(shannonStrategyCut.SpanningTrees[0], 15,  Colors.GreenYellow);
                        DrawSpanningTrees(shannonStrategyCut.SpanningTrees[1], 10,  Colors.BlueViolet);
                    }
                }
                else
                {
                    if (debugDraw)
                    {
                        DrawGraph(gridGraph, 4, Colors.Blue);
                        DrawMultiGraph(islandGraph, 7, Colors.Red);
                    }

                    if (debugDrawSpanningTrees)
                    {
                        DrawSpanningTrees(shannonStrategyShort.SpanningTrees[0], 15,  Colors.GreenYellow);
                        DrawSpanningTrees(shannonStrategyShort.SpanningTrees[1], 10,  Colors.BlueViolet);
                    }
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