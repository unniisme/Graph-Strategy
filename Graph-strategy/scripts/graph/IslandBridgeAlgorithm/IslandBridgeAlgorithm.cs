using System.Collections.Generic;

namespace Graphs
{
    public static class IslandBridgeAlgorithm<T>
    {
        /// <summary>
        /// Algorithm separates the graph into Bridges which are connected
        /// subgraphs having degree 2 or less, and Islands, which are 
        /// connected subgraphs (mostly) consistent nodes with degree 3 or more.
        /// see <see cref="Islet{T}"/>.
        /// Each island is connected to other islands by bridges
        /// </summary>
        /// <param name="startNode"></param>
        /// <returns></returns>
        public static Graph<Islet<T>> GetIslandBridgeGraph(Node<T> startNode)
        {
            return PostProcess(ConstructOverGraph(startNode));
        } 

        public static Graph<Islet<T>> ConstructOverGraph(Node<T> startNode)
        {
            Graph<Islet<T>> overGraph = new();

            Node<T> currNode = startNode;
            Islet<T> currIslet;

            if (currNode.Degree > 2)
            {
                currIslet = new(true);
                overGraph.AddNode(currIslet);
            }
            else
            {
                currIslet = new(false);
                overGraph.AddNode(currIslet);
            }
            
            ExpandNode(overGraph, currNode, currIslet);

            return overGraph;
        }

        /// <summary>
        /// Get the islet that this node belongs to
        /// Returns null if no such islet
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static Islet<T> VisitedIslet(Graph<Islet<T>> overGraph, Node<T> node)
        {
            foreach (Node<Islet<T>> islet in overGraph.Nodes)
            {
                if (islet.Data.Contains(node.Data))
                    return islet.Data;
            }
            return null;
        }

        private static void ExpandNode(Graph<Islet<T>> overGraph, Node<T> currNode, Islet<T> currIslet)
        {
            if (currIslet.Contains(currNode.Data)) return;

            // If node is already visited, it is in a different Island. Add an edge between these 2 islets
            // If the edge falls between 2 islands, they will be merged on postprocessing
            Islet<T> visitedIslet = VisitedIslet(overGraph, currNode);
            if (visitedIslet != null)
            {
                if (visitedIslet.IsIsland)
                {
                    TryAddEdge(overGraph, currIslet, visitedIslet, currIslet);
                }
                return;
            }

            // Add Deg 2 edges as bridge nodes
            // They will be converted to edges on postprocessing
            if (currIslet.IsIsland && currNode.Degree <= 2)
            {
                Islet<T> bridge = new(false);
                overGraph.AddNode(bridge);
                TryAddEdge(overGraph, bridge, currIslet, bridge);
                currIslet = bridge;
            }
            else if (!currIslet.IsIsland && currNode.Degree > 2)
            {
                Islet<T> island = new(true);
                overGraph.AddNode(island);
                TryAddEdge(overGraph, currIslet, island, currIslet);
                currIslet = island;
            }

            currIslet.Add(currNode.Data);
            foreach (Node<T> neighbor in currNode.Neighbors)
            {
                ExpandNode(overGraph, neighbor, currIslet);
            }
        }

        private static void TryAddEdge(Graph<Islet<T>> graph, Islet<T> from, Islet<T> to, Islet<T> data)
        {
            if (from == to) return;
            foreach (Edge<Islet<T>> edge in graph.DataNodeMap[from].AdjList)
            {
                if (edge.Data == data && edge.ToNode.Data == to)
                {
                    return;
                }
            }
            graph.AddEdge(from, to, data);
        }

        public static Graph<Islet<T>> PostProcess(Graph<Islet<T>> graph)
        {
            Graph<Islet<T>> finalGraph = new();

            Dictionary<Node<Islet<T>>, Node<Islet<T>>> nodeMap = new();

            HashSet<Edge<Islet<T>>> visitedEdges = new();
            
            foreach (Edge<Islet<T>> edge in graph.Edges)
            {
                // If edge data is an island, it's an edge between 2 islands
                if (edge.Data.IsIsland)
                {
                    if (visitedEdges.Contains(edge)) 
                        continue;
                    visitedEdges.Add(edge);

                    bool fromPresent = nodeMap.ContainsKey(edge.FromNode);
                    bool toPresent = nodeMap.ContainsKey(edge.ToNode);

                    // If both islands are already present, simple merge them
                    if (fromPresent && toPresent)
                    {
                        if (nodeMap[edge.FromNode] == nodeMap[edge.ToNode])
                            continue;

                        finalGraph.RemoveNode(nodeMap[edge.ToNode]);
                        nodeMap[edge.FromNode].Data.UnionWith(edge.ToNode.Data);
                        nodeMap[edge.ToNode] = nodeMap[edge.FromNode];
                    }

                    // If either of the islands are present, merge the other one into this one
                    else if (fromPresent && !toPresent)
                    {
                        nodeMap[edge.FromNode].Data.UnionWith(edge.ToNode.Data);
                        nodeMap[edge.ToNode] = nodeMap[edge.FromNode];
                    }

                    else if (!fromPresent && toPresent)
                    {
                        nodeMap[edge.ToNode].Data.UnionWith(edge.FromNode.Data);
                        nodeMap[edge.FromNode] = nodeMap[edge.ToNode];
                    }

                    // If neither islands are present, add one merged island
                    else
                    {
                        Islet<T> newIslet = edge.FromNode.Data;
                        newIslet.UnionWith(edge.ToNode.Data);

                        Node<Islet<T>> newNode = finalGraph.AddNode(newIslet);
                        nodeMap[edge.FromNode] = newNode;
                        nodeMap[edge.ToNode] = newNode;
                    }
                }
            }

            // Bridges are handled after islands are
            foreach (Edge<Islet<T>> edge in graph.Edges)
            {
                // If this edge is between an island and a bridge
                if (!edge.Data.IsIsland)
                {
                    if (visitedEdges.Contains(edge)) 
                        continue;
                    visitedEdges.Add(edge);

                    // Find the edge from the bridge to the other island
                    Edge<Islet<T>> otherEdge = edge;
                    foreach (Edge<Islet<T>> e in edge.FromNode.AdjList)
                    {
                        if (e != edge) 
                            otherEdge = e;
                    }

                    // Remove the bridge as a node and add it as and edge
                    Node<Islet<T>> fromNode = nodeMap.ContainsKey(edge.ToNode)?
                                                    nodeMap[edge.ToNode]:
                                                    finalGraph.AddNode(edge.ToNode.Data);
                    nodeMap[edge.ToNode] = fromNode;
                    

                    Node<Islet<T>> toNode = nodeMap.ContainsKey(otherEdge.ToNode)?
                                                    nodeMap[otherEdge.ToNode]:
                                                    finalGraph.AddNode(otherEdge.ToNode.Data);
                    nodeMap[otherEdge.ToNode] = toNode;

                    // If it's connected to the same island, simple merge with the island
                    if (fromNode == toNode)
                    {
                        toNode.Data.UnionWith(edge.FromNode.Data);
                    }
                    else
                    {
                        finalGraph.AddEdge(fromNode, toNode, edge.Data);
                        finalGraph.AddEdge(toNode, fromNode, edge.Data);
                        visitedEdges.Add(otherEdge);
                    }
                }
            }

            return finalGraph;

        }

    }
}