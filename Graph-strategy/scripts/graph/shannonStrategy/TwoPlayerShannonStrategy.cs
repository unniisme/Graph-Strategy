using System;
using System.Collections.Generic;
using System.Linq;
using Graphs.Search;
using Graphs.Utils;
using Logging;

namespace Graphs.Shannon
{
    public class TwoPlayerShannonStrategy<T> : IShannonStrategy<T>
    {
        private readonly Graph<T> graph;
        private readonly SpanningTree<T> spanningTreeA = new();
        private readonly SpanningTree<T> spanningTreeB = new();
        private readonly Dictionary<T,int> edgeToSpanningTree = new();
        private readonly HashSet<T> shortedEdges = new();
        private readonly HashSet<T> cutEdges = new();
        public Func<T,string> DataToString {get; set;}
        
        public bool SpanningTreesExist {get; private set;} = false;

        private List<SpanningTree<T>> SpanningTrees => new() {
            spanningTreeA,
            spanningTreeB
        };

        private static int OtherIndex(int i) => (i+1)%2;

        public TwoPlayerShannonStrategy(Graph<T> graph)
        {
            this.graph = graph;

            Logger.Inform("Initializing");
        }

        private bool TryGrowSpanningTrees(Edge<T> edge)
        {
            // Case 0: if edge can be added to either st
            foreach (SpanningTree<T> sp in SpanningTrees)
            {
                // Check if adding this edge will form a cycle in this st
                if (!sp.VertexSet.Find(edge.FromNode.Data).Equals(sp.VertexSet.Find(edge.ToNode.Data)))
                {
                    // If not, add edge to this spannign tree
                    sp.AddEdge(edge.FromNode.Data, edge.ToNode.Data, edge.Data);
                    edgeToSpanningTree[edge.Data] = SpanningTrees.IndexOf(sp);
                    Logger.Inform($"Adding Edge {DataToString(edge.Data)} to Spanning Tree {SpanningTrees.IndexOf(sp)}");
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

            int startIndex = 0;
            BFS<T> auxillarySearch = RunAuxillarySearch(auxillaryEdgeGraph, edge, startIndex);
            
            if (!auxillarySearch.Reachable)
            {
                startIndex = OtherIndex(startIndex);
                auxillarySearch = RunAuxillarySearch(auxillaryEdgeGraph, edge, startIndex);
                if (!auxillarySearch.Reachable)
                {
                    return false;
                }
            }

            foreach (Node<T> auxNode in auxillarySearch.GetPathNodes())
            {
                T edgeData = auxNode.Data;

                SpanningTree<T> currSP = SpanningTrees[edgeToSpanningTree[edgeData]];
                SpanningTree<T> otherSP = SpanningTrees[OtherIndex(edgeToSpanningTree[edgeData])];

                currSP.RemoveEdge(currSP.DataEdgeMap[edgeData]);
                Logger.Inform($"Removing Edge {DataToString(edgeData)} from Spanning Tree {edgeToSpanningTree[edgeData]}");
                otherSP.AddEdge(graph.DataEdgeMap[edgeData].FromNode.Data, graph.DataEdgeMap[edgeData].ToNode.Data, edgeData);
                edgeToSpanningTree[edgeData] = OtherIndex(edgeToSpanningTree[edgeData]);
                Logger.Inform($"Adding Edge {DataToString(edgeData)} to Spanning Tree {OtherIndex(edgeToSpanningTree[edgeData])}");
            }
            SpanningTrees[startIndex].AddEdge(edge.FromNode.Data, edge.ToNode.Data, edge.Data);
            edgeToSpanningTree[edge.Data] = startIndex;
            Logger.Inform($"Adding Edge {DataToString(edge.Data)} to Spanning Tree {startIndex}");

            return true;

        }

        private class AuxillarySearch<t> : BFS<t>
        {
            readonly int startWith = 0;
            readonly TwoPlayerShannonStrategy<t> strategy;

            public AuxillarySearch(TwoPlayerShannonStrategy<t> strategy, 
                                    SpanningTree<t> auxillaryGraph,
                                    int startWith = 0) : base(auxillaryGraph)
            {
                this.strategy = strategy;
                this.startWith = startWith;
            }

            public override void Update()
            {
                searchDict = new();
                searchTree = new();
                parentEdge = null; // No parent backtracking

                end = Start;

                ISearchCollection<t> frontier = GetFrontier();
                frontier.Add(Start);
                searchTree.AddNode(Start);
                searchDict[Start] = Start;

                while (!frontier.IsEmpty())
                {
                    t curr = frontier.Pop();

                    if (Target(curr))
                    {
                        Reachable = true;
                        end = curr;
                        return;
                    }

                    // ---- Expansion policy to find neighbors in auxillary graph-----------
                    Edge<t> edge = strategy.graph.DataEdgeMap[curr];

                    int currTree = strategy.edgeToSpanningTree.ContainsKey(curr)?
                                OtherIndex(strategy.edgeToSpanningTree[curr]):
                                startWith;

                    Logger.Debug($"Expanding {strategy.DataToString(curr)} in tree {currTree}");

                    // Find a cycle in the other tree, add all of its edges as neighbors of this edge
                    //   in the auxillary graph
                    DFS<t> findCycle = new(strategy.SpanningTrees[currTree])
                    {
                        Start = edge.FromNode.Data,
                        Target = (e) => e.Equals(edge.ToNode.Data)
                    };
                    findCycle.Update();
                    List<Edge<t>> pathEdges = findCycle.GetPathEdges();
                    Logger.Debug($"Result :\n {string.Join("\n", pathEdges.ConvertAll((e) => strategy.DataToString(e.Data)))}");

                            // Edge is not an edge of the auxillary graph
                            // But an edge in the main graph
                            // Which is a node in the auxillary graph
                    foreach (Edge<t> neighbor in pathEdges)
                    {
                        t toData = neighbor.Data;
                        if (!searchDict.ContainsKey(toData))
                        {
                            frontier.Add(toData);
                            searchTree.AddNode(toData);
                            searchTree.AddEdge(curr, toData, edge.Data);
                            searchDict[toData] = curr;
                        }
                    }
                }
            }
        }
        private BFS<T> RunAuxillarySearch(SpanningTree<T> auxillaryGraph, Edge<T> edge, int startWith = 0)
        {
            AuxillarySearch<T> auxillarySearch = new(this, auxillaryGraph, startWith)
            {
                Start = edge.Data,
                Target = (T edgeData)
                    =>
                        {
                            // Search stops if adding this edge to the other tree will not
                            //  cause a cycle to form in the other tree

                            Logger.Debug($"Checking target for {DataToString(edgeData)}");

                            int currTree = edgeToSpanningTree.ContainsKey(edgeData)?
                                        OtherIndex(edgeToSpanningTree[edgeData]):
                                        startWith;

                            DSU<T> vs = SpanningTrees[currTree].VertexSet;
                            Edge<T> edge = graph.DataEdgeMap[edgeData];
                            bool res = !vs.Find(edge.FromNode.Data).Equals(vs.Find(edge.ToNode.Data));

                            Logger.Debug($"Result : {res}");
                            return res;
                        }
            };
            auxillarySearch.Update();

            return auxillarySearch;
        }
        public Tuple<List<Edge<T>>, List<Edge<T>>> GetSpanningTrees(T a, T b)
        {
            // Initialize spanning trees
            foreach (Node<T> node in graph.Nodes)
            {
                Logger.Inform($"Node : {DataToString(node.Data)}");

                foreach (SpanningTree<T> sp in SpanningTrees)
                {
                    sp.AddNode(node.Data);
                }
            }

            // Growing spanning trees for each edge in graph
            foreach (Edge<T> edge in graph.DataEdgeMap.Values)
            {
                Logger.Debug($"Adding Edge : {DataToString(edge.Data)}");

                TryGrowSpanningTrees(edge);
            }

            ExtractIntersection();
            ExtractIntersection();

            AssertSTExistance(a, b);
            

            return new(spanningTreeA.Edges.ToList(), spanningTreeB.Edges.ToList());
        }

        private void AssertSTExistance(T a, T b)
        {
            // Check if proper spanning trees have been found
            foreach (SpanningTree<T> sp in SpanningTrees)
            {
                if (!sp.VertexSet.Find(a).Equals(sp.VertexSet.Find(b)))
                {
                    SpanningTreesExist = false;
                }
            }
            if (spanningTreeA.Edges.Count != spanningTreeB.Edges.Count)
            {
                SpanningTreesExist = false;
            }
        }

        private void ExtractIntersection()
        {
            // Extracting intersection graph
            HashSet<T> nodesA = new();
            HashSet<T> nodesB = new();
            foreach (Edge<T> edge in spanningTreeA.Edges)
            {
                nodesA.Add(edge.FromNode.Data);
                nodesA.Add(edge.ToNode.Data);
            }
            foreach (Edge<T> edge in spanningTreeB.Edges)
            {
                nodesB.Add(edge.FromNode.Data);
                nodesB.Add(edge.ToNode.Data);
            }

            HashSet<T> intersectionNodes = nodesA.Intersect(nodesB).ToHashSet();
            foreach (Node<T> node in graph.Nodes)
            {   
                if (!intersectionNodes.Contains(node.Data))
                {
                    spanningTreeA.RemoveNode(node);
                    spanningTreeB.RemoveNode(node);

                    Logger.Inform($"Removing node {DataToString(node.Data)} from STs");
                }
            }
        }

        public void Short(T edgeData)
        {
            if (shortedEdges.Contains(edgeData))
            {
                throw new GraphException<T>("Edge already shorted", graph);
            }

            if (spanningTreeA.DataEdgeMap.ContainsKey(edgeData) || spanningTreeB.DataEdgeMap.ContainsKey(edgeData))
            {
                shortedEdges.Add(edgeData);
            }
            else
            {
                throw new GraphException<T>("Edge not present in either spanningTree", spanningTreeA);
            }
        }

        public void Cut(T edgeData)
        {
            if (cutEdges.Contains(edgeData))
            {
                throw new GraphException<T>("Edge already cut", graph);
            }

            if (spanningTreeA.DataEdgeMap.ContainsKey(edgeData) || spanningTreeB.DataEdgeMap.ContainsKey(edgeData))
            {
                cutEdges.Add(edgeData);
            }
            else
            {
                throw new GraphException<T>("Edge not present in either spanningTree", spanningTreeA);
            }
        }

        public T GetShortMove()
        {
            if (!SpanningTreesExist)
            {
                // For now random move
                return spanningTreeA.Nodes.ElementAt(0).Data;
            }

            foreach (T cutEdgeData in cutEdges)
            {
                if (spanningTreeA.DataEdgeMap.ContainsKey(cutEdgeData))
                {
                    spanningTreeA.RemoveEdge(spanningTreeA.DataEdgeMap[cutEdgeData]);
                    return FindShortEdge(1);
                }
                else if (spanningTreeB.DataEdgeMap.ContainsKey(cutEdgeData))
                {
                    spanningTreeB.RemoveEdge(spanningTreeB.DataEdgeMap[cutEdgeData]);
                    return FindShortEdge(0);
                }
            }

            return spanningTreeA.Nodes.ElementAt(0).Data;
        }

        private T FindShortEdge(int tree)
        {
            DSU<T> otherVS = SpanningTrees[OtherIndex(tree)].VertexSet;
            foreach (Edge<T> e in SpanningTrees[tree].Edges)
            {
                if (shortedEdges.Contains(e.Data) || cutEdges.Contains(e.Data))
                    continue;

                if (!otherVS.Find(e.FromNode.Data).Equals(otherVS.Find(e.ToNode.Data)))
                    return e.Data;
            }
            return spanningTreeA.Nodes.ElementAt(0).Data;
        }
    }
}