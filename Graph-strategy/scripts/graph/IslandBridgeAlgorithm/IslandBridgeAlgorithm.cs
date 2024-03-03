using System.Collections.Generic;

namespace Graphs
{
    public class IslandBridgeAlgorithm<T>
    {
        Graph<Islet<T>> overGraph;
        public SimpleGraph<Islet<T>> GetIslandBridgeGraph(Node<T> startNode)
        {
            overGraph = new();

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
            
            ExpandNode(currNode, currIslet);

            // Code to Complete islet graphs here            

            return PostProcess(overGraph);
        } 

        /// <summary>
        /// Get the islet that this node belongs to
        /// Returns null if no such islet
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private Islet<T> VisitedIslet(Node<T> node)
        {
            foreach (Node<Islet<T>> islet in overGraph.Nodes)
            {
                if (islet.Data.Nodes.Contains(node))
                    return islet.Data;
            }
            return null;
        }

        private void ExpandNode(Node<T> currNode, Islet<T> currIslet)
        {
            if (currIslet.Nodes.Contains(currNode)) return;

            // If node is already visited, it is in a different Island. Add an edge between these 2 islets
            // If the edge falls between 2 islands, they will be merged on postprocessing
            Islet<T> visitedIslet = VisitedIslet(currNode);
            if (visitedIslet != null)
            {
                try
                {
                    overGraph.AddEdge(currIslet, visitedIslet, currIslet);
                }
                catch (GraphException<T>)
                {
                    // If edge already present, ignore
                }
                return;
            }

            // Add Deg 2 edges as bridge nodes
            // They will be converted to edges on postprocessing
            if (currIslet.IsIsland && currNode.Degree <= 2)
            {
                Islet<T> bridge = new(false);
                overGraph.AddNode(bridge);
                overGraph.AddEdge(bridge, currIslet, bridge);
                currIslet = bridge;
            }
            else if (!currIslet.IsIsland && currNode.Degree > 2)
            {
                Islet<T> island = new(true);
                overGraph.AddNode(island);
                overGraph.AddEdge(island, currIslet, currIslet);
                currIslet = island;
            }

            currIslet.AddNode(currNode);
            foreach (Node<T> neighbor in currNode.Neighbors)
            {
                ExpandNode(neighbor, currIslet);
            }
        }

        private static SimpleGraph<Islet<T>> PostProcess(Graph<Islet<T>> graph)
        {
            SimpleGraph<Islet<T>> finalGraph = new();

            Dictionary<Node<Islet<T>>, Node<Islet<T>>> nodeMap = new();
            HashSet<Edge<Islet<T>>> visitedEdges = new();
            
            foreach (Edge<Islet<T>> edge in graph.Edges)
            {
                if (visitedEdges.Contains(edge)) 
                    continue;
                visitedEdges.Add(edge);

                // If edge data is an island, it's an edge between 2 islands
                if (edge.Data.IsIsland)
                {
                    bool fromPresent = nodeMap.ContainsKey(edge.FromNode);
                    bool toPresent = nodeMap.ContainsKey(edge.ToNode);

                    // If both islands are already present, simple merge them
                    if (fromPresent && toPresent)
                    {
                        if (nodeMap[edge.FromNode] == nodeMap[edge.ToNode])
                            continue;

                        finalGraph.RemoveNode(nodeMap[edge.ToNode]);
                        edge.FromNode.Data.JoinNodes(edge.ToNode.Data);
                        nodeMap[edge.ToNode] = nodeMap[edge.FromNode];
                    }

                    // If either of the islands are present, merge the other one into this one
                    else if (fromPresent && !toPresent)
                    {
                        edge.FromNode.Data.JoinNodes(edge.ToNode.Data);
                        nodeMap[edge.ToNode] = nodeMap[edge.FromNode];
                    }

                    else if (!fromPresent && toPresent)
                    {
                        edge.ToNode.Data.JoinNodes(edge.FromNode.Data);
                        nodeMap[edge.FromNode] = nodeMap[edge.ToNode];
                    }

                    // If neither islands are present, add one merged island
                    else
                    {
                        Islet<T> newIslet = edge.FromNode.Data;
                        newIslet.JoinNodes(edge.ToNode.Data);

                        Node<Islet<T>> newNode = finalGraph.AddNode(newIslet);
                        nodeMap[edge.FromNode] = newNode;
                        nodeMap[edge.ToNode] = newNode;
                    }
                }

                // If this edge is between an island and a bridge
                else
                {
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

                    finalGraph.AddEdge(fromNode, toNode, edge.Data);
                }
            }

            return finalGraph;

        }

    }
}