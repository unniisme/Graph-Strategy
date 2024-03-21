using System;
using System.Collections.Generic;
using Graphs.Utils;

namespace Graphs.Shannon
{
    public class ShannonStrategy<T>
    {
        private readonly Graph<T> graph;

        public ShannonStrategy(Graph<T> graph)
        {
            this.graph = graph;
        }

        public Dictionary<Node<T>, Node<T>> ParentTree(Node<T> x, Dictionary<Edge<T>, bool> edgeIsInForest)
        {   
            Dictionary<Node<T>, Node<T>> parentDict = new()
            {
                [x] = x
            };

            Queue<Node<T>> frontier = new();
            frontier.Enqueue(x);

            while (frontier.Count > 0)
            {
                Node<T> currnode = frontier.Dequeue();
                foreach (Edge<T> edge in currnode.AdjList)
                {
                    if (edgeIsInForest.GetValueOrDefault(edge) && !parentDict.ContainsKey(edge.ToNode))
                    {
                        parentDict[edge.ToNode] = currnode;
                        frontier.Enqueue(edge.ToNode);
                    }
                }
            }

            return parentDict;
        }


        public Tuple<List<Edge<T>>, List<Edge<T>>> GetMove()
        {
            DSU<Node<T>> clumps = new();
            DSU<Node<T>> forest0 = new();
            DSU<Node<T>> forest1 = new();

            Dictionary<Edge<T>, bool> edgeIsInForest0 = new();


            foreach (Node<T> node in graph.Nodes)
            {
                clumps.MakeSet(node);
                forest0.MakeSet(node);
                forest1.MakeSet(node);
            }

            foreach (Edge<T> xy in graph.Edges)
            {
                if (clumps.Find(xy.FromNode) == clumps.Find(xy.ToNode))
                {
                    continue;
                }

                Queue<Edge<T>> edgeQueue = new();
                Dictionary<Edge<T>, Edge<T>> edgeLabel = new()
                {
                    [xy] = null
                };

                edgeQueue.Enqueue(xy);

                Dictionary<Node<T>,Node<T>> parentTree0 = ParentTree(xy.FromNode, edgeIsInForest0);
                Dictionary<Node<T>,Node<T>> parentTree1 = ParentTree(xy.FromNode, ReverseDict(edgeIsInForest0));

                Edge<T> augmentEdge = null;
                DSU<Node<T>> forest = null; // final Fi 
                bool isForest0 = true;
                Dictionary<Node<T>,Node<T>> parentTree;
                // To find augmenting sequence
                while (edgeQueue.Count > 0)
                {
                    Edge<T> uv = edgeQueue.Dequeue();

                    forest = forest0;
                    parentTree = parentTree0;
                    isForest0 = true;
                    if (edgeIsInForest0.ContainsKey(uv))
                    {
                        if (!edgeIsInForest0[uv])
                        {
                            forest = forest1;
                            parentTree = parentTree1;
                            isForest0 = false;
                        }
                    }


                    if (forest.Find(uv.FromNode) != forest.Find(uv.ToNode))
                    {
                        augmentEdge = uv;
                        break;
                    }
                    else
                    {
                        Stack<Edge<T>> labelStack = new();

                        Node<T> currNode = uv.FromNode; // currNode should not be in the subtree of labelled edges.

                        Edge<T> parentEdge = graph.GetEdge(currNode, parentTree[currNode]);
                        Edge<T> parentLabel = (parentEdge==null)?null:edgeLabel.GetValueOrDefault(parentEdge);
                        while (currNode != xy.FromNode || parentLabel != null)
                        {
                            labelStack.Push(graph.GetEdge(currNode, parentTree[currNode]));
                            currNode = parentTree[currNode];

                            parentEdge = graph.GetEdge(currNode, parentTree[currNode]);
                            parentLabel = (parentEdge==null)?null:edgeLabel.GetValueOrDefault(parentEdge);
                        }

                        while (labelStack.Count > 0)
                        {
                            Edge<T> currEdge = labelStack.Pop();
                            edgeLabel[currEdge] = uv;
                            edgeQueue.Enqueue(currEdge);
                        }
                    }
                }

                // No augmenting path
                if (augmentEdge == null)
                {
                    forest0.Union(xy.FromNode, xy.ToNode);
                }
                else
                {
                    forest.Union(augmentEdge.FromNode, augmentEdge.ToNode);

                    edgeIsInForest0[augmentEdge] = isForest0;

                    while (edgeLabel[augmentEdge] != null)
                    {
                        isForest0 = edgeIsInForest0[edgeLabel[augmentEdge]];
                        edgeIsInForest0[edgeLabel[augmentEdge]] = edgeIsInForest0[augmentEdge];
                        edgeIsInForest0[augmentEdge] = isForest0;
                        augmentEdge = edgeLabel[augmentEdge];
                    }
                }
            }

            List<Edge<T>> spanningTree0 = new();
            List<Edge<T>> spanningTree1 = new();

            foreach (KeyValuePair<Edge<T>, bool> kvp in edgeIsInForest0)
            {
                if (kvp.Value)
                    spanningTree0.Add(kvp.Key);
                else
                    spanningTree1.Add(kvp.Key);
            }
            // foreach (Edge<T> edge in graph.Edges)
            // {
            //     if (!edgeIsInForest0.ContainsKey(edge))
            //         spanningTree1.Add(edge);
            // }
            return new(spanningTree0, spanningTree1);
        }

        private static Dictionary<Edge<T>, bool> ReverseDict(Dictionary<Edge<T>, bool> edgeIs)
        {
            Dictionary<Edge<T>, bool> outDict = new();
            foreach (KeyValuePair<Edge<T>,bool> kvp in edgeIs)
            {
                outDict[kvp.Key] = !kvp.Value;
            }
            return outDict;
        }
    }
}