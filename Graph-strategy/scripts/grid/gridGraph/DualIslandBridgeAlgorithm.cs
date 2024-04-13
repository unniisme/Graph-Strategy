using System.Collections.Generic;
using Godot;
using Graphs;
using Logging;

namespace Gamelogic.Grid.Graph
{
    public class DualIslandBridgeGraph : IslandBridgeGraph<Vector2I>
    {
        private readonly IslandBridgeGraph<Vector2I> primal = null;
        public DualIslandBridgeGraph(Node<Vector2I> start, IslandBridgeGraph<Vector2I> primal) 
        {
            this.primal = primal;
            PostProcess(ConstructOverGraph(start));
        }

        public DualIslandBridgeGraph(Node<Vector2I> start, IslandBridgeGraph<Vector2I> primal, Logger logger) 
        {
            Trace = logger;
            this.primal = primal;
            PostProcess(ConstructOverGraph(start));
        }

        internal override bool IslandCondition(Node<Vector2I> node)
        {
            return !primal.islets.ContainsElement(node.Data);
        }

        internal override void PostProcess(Graph<Vector2I> graph)
        {
            foreach (Node<Vector2I> node in graph.Nodes)
            {
                Vector2I islet = node.Data;
                if (islets.IsIsland(islet))
                {
                    if (!DataNodeMap.ContainsKey(islets.Find(islet)))
                        AddNode(islets.Find(islet));

                    foreach (Edge<Vector2I> edge in node.AdjList)
                    {
                        if (islets.IsIsland(edge.Data))
                        {
                            if (DataNodeMap.ContainsKey(edge.FromNode.Data))
                                RemoveNode(islets.Find(edge.FromNode.Data));
                            if (DataNodeMap.ContainsKey(edge.ToNode.Data))
                                RemoveNode(islets.Find(edge.ToNode.Data));

                            islets.Union(edge.ToNode.Data, edge.FromNode.Data);
                            if (!DataNodeMap.ContainsKey(islets.Find(edge.ToNode.Data)))
                                AddNode(islets.Find(edge.FromNode.Data));
                        }
                    }
                }
            }

            // Bridges are handled after islands are
            // Bridges in overGraph are necessarily bridges in dual
            foreach (Node<Vector2I> node in graph.Nodes)
            {
                Vector2I islet = node.Data;
                if (!islets.IsIsland(islet))
                {
                    islet = primal.islets.Find(islet);
                    if (islets.Find(node.AdjList[0].ToNode.Data).Equals(islets.Find(node.AdjList[1].ToNode.Data)))
                            continue;

                    TryAddEdge(this,
                        islets.Find(node.AdjList[0].ToNode.Data), 
                        islets.Find(node.AdjList[1].ToNode.Data),
                        islets.Find(islet)
                        );
                }
            }
        }
    }
}