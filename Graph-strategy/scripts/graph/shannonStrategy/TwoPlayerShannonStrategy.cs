using System;
using System.Collections.Generic;
using System.Linq;
using Graphs.Search;
using Graphs.Utils;

namespace Graphs.Shannon
{
    public class TwoPlayerShannonStrategy<T> : IShannonStrategy<T>
    {
        private readonly Graph<T> graph;
        private readonly SpanningTree<T> spanningTreeA = new();

        private readonly SpanningTree<T> spanningTreeB = new();

        private List<SpanningTree<T>> SpanningTrees => new() {
            spanningTreeA,
            spanningTreeB
        };

        public TwoPlayerShannonStrategy(Graph<T> graph)
        {
            this.graph = graph;

            // Initialize spanning trees
            foreach (Node<T> node in graph.Nodes)
            {
                foreach (SpanningTree<T> sp in SpanningTrees)
                {
                    sp.AddNode(node.Data);
                }
            }
        }

        int currTree = 0;
        private bool TryGrowSpanningTrees(Edge<T> edge)
        {
            // Case 0: if edge can be added to either st
            // Note, this case should be a subset of the next case
            foreach (SpanningTree<T> sp in SpanningTrees)
            {
                if (!sp.VertexSet.Find(edge.FromNode.Data).Equals(sp.VertexSet.Find(edge.ToNode.Data)))
                {
                    sp.AddEdge(edge.FromNode, edge.ToNode, edge.Data);
                    return true;
                }
            }

            // Case 1: If the edge can't be added to either st
            
            // Maintain an auxillary graph to search for augmenting edges
            SpanningTree<T> auxillaryEdgeGraph = new();
            foreach (Edge<T> e in graph.Edges)
            {
                if (!auxillaryEdgeGraph.DataNodeMap.ContainsKey(e.Data))
                    auxillaryEdgeGraph.AddNode(e.Data);
            }

            BFS<T> auxillarySearch = new(auxillaryEdgeGraph)
            {
                GetAdjList = (T data) 
                    => 
                        {
                            Edge<T> edge = graph.DataEdgeMap[data];
                            DFS<T> findCycle = new(SpanningTrees[currTree])
                            {
                                Start = edge.FromNode.Data,
                                Target = (e) => e.Equals(edge.ToNode.Data)
                            };
                            findCycle.Update();
                            currTree = (currTree + 1)%2;
                            return findCycle.GetPathEdges();
                        },
                Start = edge.Data,
                Target = (T edgeData)
                    =>
                        {
                            DSU<T> vs = SpanningTrees[currTree].VertexSet;
                            Edge<T> edge = graph.DataEdgeMap[edgeData];
                            return !vs.Find(edge.FromNode.Data).Equals(vs.Find(edge.ToNode.Data));
                        }
            };
            
            auxillarySearch.Update();

            if (!auxillarySearch.Reachable) return false;

            foreach (Node<T> auxNode in auxillarySearch.GetPathNodes())
            {
                T edgeData = auxNode.Data;

                SpanningTree<T> currSP = SpanningTrees[currTree];
                SpanningTree<T> otherSP = SpanningTrees[(currTree+1)%2];

                currSP.RemoveEdge(currSP.DataEdgeMap[edgeData]);
                otherSP.AddEdge(graph.DataEdgeMap[edgeData].FromNode, graph.DataEdgeMap[edgeData].ToNode, edgeData);

                currTree = (currTree+1)%2;
            }
            SpanningTrees[currTree].AddEdge(edge.FromNode, edge.ToNode, edge.Data);

            return true;

        }
        public Tuple<List<Edge<T>>, List<Edge<T>>> GetSpanningTrees()
        {
            foreach (Edge<T> edge in graph.DataEdgeMap.Values)
            {
                TryGrowSpanningTrees(edge);
            }
            return new(spanningTreeA.Edges.ToList(), spanningTreeB.Edges.ToList());
        }
    }
}