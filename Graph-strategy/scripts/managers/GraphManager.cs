using System;
using System.Collections.Generic;
using Gamelogic.Grid;
using Godot;
using Graphs;
using Graphs.Shannon;

namespace Gamelogic.Managers
{
    [GlobalClass]
    public partial class GraphManager : Node2D
    {
        private NavigationGridGraph gridGraph = null;
        private IGrid grid = null;
        private Graph<Islet<Vector2I>> islandGraph = null;
        private Tuple<List<Edge<Islet<Vector2I>>>, List<Edge<Islet<Vector2I>>>> spanningTrees = null;

        bool draw = false;

        [Export]
        public bool debugWrite = false;
        [Export]
        public bool debugDraw = false;
        [Export]
        public bool debugDrawSpanningTrees = true;

        [Export]
        public Node2D gridSeachStartTarget;

        public void Debug()
        {
            if (grid == null)
            {
                CalculateIslands();
            }

            draw = !draw;
            QueueRedraw();
        }

        public void CalculateIslands()
        {
            grid ??= GameManager.GetLevel().grid;
            Vector2I pos = grid.GameCoordinateToGridCoordinate(gridSeachStartTarget.Position);

            gridGraph = new(grid, pos);
            islandGraph = IslandBridgeAlgorithm<Vector2I>.GetIslandBridgeGraph(gridGraph.DataNodeMap[pos]); 

            ShannonStrategy<Islet<Vector2I>> agent = new(islandGraph);
            spanningTrees = agent.GetSpanningTrees();

            if (debugWrite) 
                GD.Print(ShowIslandGraph(islandGraph));
        }

        private void DrawIslands()
        {
            foreach (Islet<Vector2I> islet in islandGraph.DataNodeMap.Keys)
            {
                DrawGraph(islet, 5, Colors.Green);
            }
            foreach (Edge<Islet<Vector2I>> edge in islandGraph.Edges)
            {
                DrawGraph(edge.Data, 3, Colors.Red);
            }
        }

        private void DrawEdges(List<Edge<Islet<Vector2I>>> edges, Color color)
        {
            foreach (Edge<Islet<Vector2I>> edge in edges)
            {
                DrawGraph(edge.Data, 3, color);
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
                    DrawGraph(gridGraph, 4, Colors.Blue);
                    DrawIslands();
                }

                if (debugDrawSpanningTrees)
                {
                    DrawEdges(spanningTrees.Item1, Colors.GreenYellow);
                    DrawEdges(spanningTrees.Item2, Colors.AliceBlue);
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
            foreach (Node<Vector2I> pos in islet.Nodes)
            {
                isletString = $"{isletString} {pos.Data}";
            }
            return isletString;
        }
    }
}