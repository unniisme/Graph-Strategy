using System.Collections.Generic;
using Godot;
using Graphs;

namespace Gamelogic.Grid.Graph
{
    public static class DualIslandBridgeAlgorithm // : IslandBridgeAlgorithm<Vector2I>
    {
        public static Graph<Islet<Vector2I>> GetIslandBridgeGraph(Node<Vector2I> startNode, Graph<Islet<Vector2I>> primal)
        {
            return IslandBridgeAlgorithm<Vector2I>.PostProcess(ConstructOverGraph(startNode, primal));
        } 
        public static Graph<Islet<Vector2I>> ConstructOverGraph(Node<Vector2I> startNode, Graph<Islet<Vector2I>> primal)
        {
            Graph<Islet<Vector2I>> overGraph = new();

            Node<Vector2I> currNode = startNode;
            Islet<Vector2I> currIslet;

            HashSet<Vector2I> edgePoses = new();
            foreach (Edge<Islet<Vector2I>> islet in primal.Edges)
            {
                edgePoses.UnionWith(islet.Data);
            } 

            if (edgePoses.Contains(currNode.Data))
            {
                currIslet = new(true);
                overGraph.AddNode(currIslet);
            }
            else
            {
                currIslet = new(false);
                overGraph.AddNode(currIslet);
            }
            
            ExpandNode(overGraph, currNode, currIslet, edgePoses);

            return overGraph;
        }


        private static void ExpandNode(Graph<Islet<Vector2I>> overGraph, Node<Vector2I> currNode, Islet<Vector2I> currIslet,
                                        HashSet<Vector2I> edgePoses)
        {
            if (currIslet.Contains(currNode.Data)) return;

            // If node is already visited, it is in a different Island. Add an edge between these 2 islets
            // If the edge falls between 2 islands, they will be merged on postprocessing
            Islet<Vector2I> visitedIslet = IslandBridgeAlgorithm<Vector2I>.VisitedIslet(overGraph, currNode);
            if (visitedIslet != null)
            {
                if (visitedIslet.IsIsland)
                {
                    IslandBridgeAlgorithm<Vector2I>.TryAddEdge(overGraph, currIslet, visitedIslet, currIslet);
                }
                return;
            }

            if (currIslet.IsIsland && edgePoses.Contains(currNode.Data))
            {
                Islet<Vector2I> bridge = new(false);
                overGraph.AddNode(bridge);
                IslandBridgeAlgorithm<Vector2I>.TryAddEdge(overGraph, bridge, currIslet, bridge);
                currIslet = bridge;
            }
            else if (!currIslet.IsIsland && !edgePoses.Contains(currNode.Data))
            {
                Islet<Vector2I> island = new(true);
                overGraph.AddNode(island);
                IslandBridgeAlgorithm<Vector2I>.TryAddEdge(overGraph, currIslet, island, currIslet);
                currIslet = island;
            }

            currIslet.Add(currNode.Data);
            foreach (Node<Vector2I> neighbor in currNode.Neighbors)
            {
                ExpandNode(overGraph, neighbor, currIslet, edgePoses);
            }
        }

        public static Graph<Islet<Vector2I>> PostProcess(Graph<Islet<Vector2I>> graph)
        {
            Graph<Islet<Vector2I>> finalGraph = new();

            Dictionary<Node<Islet<Vector2I>>, Node<Islet<Vector2I>>> nodeMap = new();

            HashSet<Edge<Islet<Vector2I>>> visitedEdges = new();
            
            foreach (Edge<Islet<Vector2I>> edge in graph.Edges)
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
                        Islet<Vector2I> newIslet = edge.FromNode.Data;
                        newIslet.UnionWith(edge.ToNode.Data);

                        Node<Islet<Vector2I>> newNode = finalGraph.AddNode(newIslet);
                        nodeMap[edge.FromNode] = newNode;
                        nodeMap[edge.ToNode] = newNode;
                    }
                }
            }

            // Bridges are handled after islands are
            foreach (Edge<Islet<Vector2I>> edge in graph.Edges)
            {
                // If this edge is between an island and a bridge
                if (!edge.Data.IsIsland)
                {
                    if (visitedEdges.Contains(edge)) 
                        continue;
                    visitedEdges.Add(edge);

                    // Find the edge from the bridge to the other island
                    Edge<Islet<Vector2I>> otherEdge = edge;
                    foreach (Edge<Islet<Vector2I>> e in edge.FromNode.AdjList)
                    {
                        if (e != edge) 
                            otherEdge = e;
                    }

                    // Remove the bridge as a node and add it as and edge
                    Node<Islet<Vector2I>> fromNode = nodeMap.ContainsKey(edge.ToNode)?
                                                    nodeMap[edge.ToNode]:
                                                    finalGraph.AddNode(edge.ToNode.Data);
                    nodeMap[edge.ToNode] = fromNode;
                    

                    Node<Islet<Vector2I>> toNode = nodeMap.ContainsKey(otherEdge.ToNode)?
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