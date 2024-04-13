using System.Collections.Generic;
using Graphs.Utils;
using Logging;

namespace Graphs
{
    public class IslandBridgeGraph<T> : Graph<T>
    {
        public readonly Islet<T> islets = new();


        /// <summary>
        /// Algorithm separates the graph into Bridges which are connected
        /// subgraphs having degree 2 or less, and Islands, which are 
        /// connected subgraphs (mostly) consistent nodes with degree 3 or more.
        /// see <see cref="Islet{T}"/>.
        /// Each island is connected to other islands by bridges
        /// </summary>
        /// <param name="startNode"></param>
        /// <returns></returns>
        public IslandBridgeGraph(Node<T> startNode)
        {
            PostProcess(ConstructOverGraph(startNode));
        }

        public IslandBridgeGraph(Node<T> startNode, Logger logger)
        {
            Trace = logger;
            PostProcess(ConstructOverGraph(startNode));
        }

        public IslandBridgeGraph() {}

        public Graph<T> ConstructOverGraph(Node<T> startNode)
        {
            Graph<T> overGraph = new();
            Node<T> currNode = startNode;

            T currIslet = startNode.Data;

            if (IslandCondition(currNode))
            {
                islets.MakeSet(currIslet, true);
                overGraph.AddNode(currIslet);
            }
            else
            {
                islets.MakeSet(currIslet, false);
                overGraph.AddNode(currIslet);
            }
            
            foreach (Node<T> neighbor in currNode.Neighbors)
            {
                ExpandNode(overGraph, neighbor, currIslet);
            }

            return overGraph;
        }

        internal void ExpandNode(Graph<T> overGraph, Node<T> currNode, T currIslet)
        {
            if (islets.ContainsElement(currNode.Data))
            {
                T visitedIslet = islets.Find(currNode.Data);
                if (currIslet.Equals(visitedIslet)) return;

                // If node is already visited, it is in a different Island. Add an edge between these 2 islets
                // If the edge falls between 2 islands, they will be merged on postprocessing
                if (islets.IsIsland(visitedIslet))
                {
                    TryAddEdge(overGraph, currIslet, visitedIslet, currIslet);
                }
                return;
            }

            islets.MakeSet(currNode.Data);

            // Add Deg 2 edges as bridge nodes
            // They will be converted to edges on postprocessing
            if (islets.IsIsland(currIslet) && !IslandCondition(currNode))
            {
                islets.MakeSet(currNode.Data, false);
                T bridge = currNode.Data;
                overGraph.AddNode(bridge);
                TryAddEdge(overGraph, bridge, currIslet, bridge);
                currIslet = bridge;
            }
            else if (!islets.IsIsland(currIslet) && IslandCondition(currNode))
            {
                islets.MakeSet(currNode.Data, true);
                T island = currNode.Data;
                overGraph.AddNode(island);
                TryAddEdge(overGraph, currIslet, island, currIslet);
                currIslet = island;
            }
            else
            {
                islets.LeftUnion(currIslet, currNode.Data);
            }

            foreach (Node<T> neighbor in currNode.Neighbors)
            {
                ExpandNode(overGraph, neighbor, currIslet);
            }
        }

        internal static void TryAddEdge(Graph<T> graph, T from, T to, T data)
        {
            if (from.Equals(to)) return;
            foreach (Edge<T> edge in graph.DataNodeMap[from].AdjList)
            {
                if (edge.Data.Equals(data) && edge.ToNode.Data.Equals(to))
                {
                    return;
                }
            }
            graph.AddEdge(from, to, data);
        }

        internal virtual void PostProcess(Graph<T> graph)
        {
            HashSet<Edge<T>> visitedEdges = new();
            
            foreach (Edge<T> edge in graph.Edges)
            {
                // If edge data is an island, it's an edge between 2 islands
                if (islets.IsIsland(edge.Data))
                {
                    if (visitedEdges.Contains(edge)) 
                        continue;
                    visitedEdges.Add(edge);

                    if (DataNodeMap.ContainsKey(edge.FromNode.Data))
                        RemoveNode(islets.Find(edge.FromNode.Data));
                    if (DataNodeMap.ContainsKey(edge.ToNode.Data))
                        RemoveNode(islets.Find(edge.ToNode.Data));

                    islets.Union(edge.ToNode.Data, edge.FromNode.Data);
                    if (!DataNodeMap.ContainsKey(islets.Find(edge.ToNode.Data)))
                        AddNode(islets.Find(edge.FromNode.Data));
                }
            }

            // Bridges are handled after islands are
            foreach (Node<T> node in graph.Nodes)
            {
                T islet = node.Data;
                if (!islets.IsIsland(islet))
                {
                    if (node.Degree < 2)
                    {
                        islets.LeftUnion(node.AdjList[0].ToNode.Data, islet);
                    }
                    else
                    {
                        if (islets.Find(node.AdjList[0].ToNode.Data).Equals(islets.Find(node.AdjList[1].ToNode.Data)))
                        {
                            islets.LeftUnion(node.AdjList[0].ToNode.Data, islet);
                            continue;
                        }

                        AddEdge(
                            islets.Find(node.AdjList[0].ToNode.Data), 
                            islets.Find(node.AdjList[1].ToNode.Data),
                            islets.Find(islet)
                            );
                    }
                }
            }
        }


        internal virtual bool IslandCondition(Node<T> node)
        {
            return node.Degree > 2;
        }

        public override Node<T> ShortEdge(Edge<T> edge)
        {
            Dictionary<T,T> fromEdges = new();
            Dictionary<T,T> toEdges = new();

            foreach (Edge<T> e in Edges)
            {
                if (e.FromNode.Data.Equals(edge.FromNode.Data) || e.FromNode.Data.Equals(edge.ToNode.Data))
                {
                    if (e.ToNode != edge.FromNode && e.ToNode != edge.ToNode)
                        toEdges[e.Data] = e.ToNode.Data;
                }
                if (e.ToNode.Data.Equals(edge.FromNode.Data) || e.ToNode.Data.Equals(edge.ToNode.Data))
                {
                    if (e.FromNode != edge.FromNode && e.FromNode != edge.ToNode)
                        fromEdges[e.Data] = e.FromNode.Data;
                }
            }

            RemoveNode(edge.FromNode);
            RemoveNode(edge.ToNode);

            islets.Union(edge.FromNode.Data, edge.ToNode.Data);

            Node<T> newNode = AddNode(islets.Find(edge.FromNode.Data));
            
            foreach (KeyValuePair<T,T> edgeNeighbor in fromEdges)
            {
                AddEdge(edgeNeighbor.Value, newNode.Data, edgeNeighbor.Key);
            }
            foreach (KeyValuePair<T,T> edgeNeighbor in toEdges)
            {
                AddEdge(newNode.Data, edgeNeighbor.Value, edgeNeighbor.Key);
            }

            return newNode;
        }

    }
}