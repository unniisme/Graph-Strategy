using System;
using System.Collections.Generic;
using Godot;
using Graphs;
using Logging;

namespace Gamelogic.Grid.Graph
{
    public class DualIslandBridgeGraph : IslandBridgeGraph<Vector2I>
    {
        private readonly IslandBridgeGraph<Vector2I> primal = null;

        private Vector2I upperIslet;
        private Vector2I? lowerIslet;

        public DualIslandBridgeGraph(Node<Vector2I> outer, Node<Vector2I> inner, IslandBridgeGraph<Vector2I> primal, Logger logger = null) 
        {
            Trace = logger;
            this.primal = primal;

            PostProcess(ConstructOverGraph(inner, PreProcess(outer, inner, new())));
        }

        internal Graph<Vector2I> PreProcess(Node<Vector2I> outer, Node<Vector2I> inner, Graph<Vector2I> overGraph)
        {
            Node<Vector2I> currNode = outer;

            upperIslet = outer.Data;

            islets.MakeSet(upperIslet, true);   
            
            foreach (Node<Vector2I> neighbor in currNode.Neighbors)
            {
                ExpandNodePre(neighbor, 
                                (Vector2I pos) => ((Vector2)pos - outer.Data).Cross(outer.Data - inner.Data) >= 0);
            }


            upperIslet = islets.Find(upperIslet);
            lowerIslet = islets.Find((Vector2I)lowerIslet);
            overGraph.AddNode(upperIslet);
            overGraph.AddNode((Vector2I)lowerIslet);

            Trace.Inform($"upperIsland : {upperIslet}, lowerIslet : {lowerIslet}");

            return overGraph;
        }

        internal void ExpandNodePre(Node<Vector2I> curr, Func<Vector2I,bool> checkUpper)
        {
            if (primal.islets.ContainsElement(curr.Data) || islets.ContainsElement(curr.Data))
                return;

            if (!islets.ContainsElement(curr.Data))
            {
                islets.MakeSet(curr.Data, true);

                if (checkUpper(curr.Data))
                {
                    islets.Union(upperIslet, curr.Data);
                    upperIslet = islets.Find(upperIslet);
                }
                else
                {
                    if (lowerIslet == null)
                    {
                        lowerIslet = curr.Data;
                    }
                    else
                    {
                        islets.Union((Vector2I)lowerIslet, curr.Data);
                        lowerIslet = islets.Find((Vector2I)lowerIslet);
                    }
                }
            }

            foreach (Node<Vector2I> neighbor in curr.Neighbors)
            {
                ExpandNodePre(neighbor, checkUpper);
            }

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
                            if (islets.Find(edge.FromNode.Data).Equals(islets.Find(upperIslet)) 
                            || islets.Find(edge.FromNode.Data).Equals(islets.Find((Vector2I)lowerIslet)))
                                continue;

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