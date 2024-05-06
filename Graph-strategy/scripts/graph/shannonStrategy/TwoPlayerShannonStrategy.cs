using System;
using System.Collections.Generic;
using Graphs.Search;
using Graphs.Utils;
using Logging;

namespace Graphs.Shannon
{
    public class TwoPlayerShannonStrategy<T> : IShannonStrategy<T>
    {
        public virtual Logger Trace { get; set; } = new Logger("TwoPlayerShannonStrategy");

        internal Graph<T> graph;
        internal SpanningTree<T> spanningTreeA = new();
        internal SpanningTree<T> spanningTreeB = new();
        internal readonly Dictionary<T,int> edgeToSpanningTree = new();
        internal readonly HashSet<T> shortedEdges = new();
        public Func<T,string> DataToString {get; set;} = (t) => $"{t}";
        
        public List<SpanningTree<T>> SpanningTrees => new() {
            spanningTreeA,
            spanningTreeB
        };

        public T ShortMove {get; internal set;} = default;

        internal static int OtherIndex(int i) => (i+1)%2;

        internal T source;
        internal T sink;

        public TwoPlayerShannonStrategy() {}

        public TwoPlayerShannonStrategy(Graph<T> graph, T source, T sink)
        {
            this.graph = graph;
            this.source = source;
            this.sink = sink;

            Clear();

            Trace.Inform("Initializing");
        }

        public void Clear()
        {
            spanningTreeA = new()
            {
                Trace = new(Trace, "spanning tree A")
            };
            spanningTreeB = new()
            {
                Trace = new(Trace, "spanning tree B")
            };
            edgeToSpanningTree.Clear();
            shortedEdges.Clear();
        }

        internal bool TryGrowSpanningTrees(Edge<T> edge)
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
                    Trace.Inform($"Adding Edge {DataToString(edge.Data)} to Spanning Tree {SpanningTrees.IndexOf(sp)}");
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
                    Trace.Warn($"Not possible to add edge {edge.Data} to either spanning tree");
                    return false;
                }
            }

            foreach (Node<T> auxNode in auxillarySearch.GetPathNodes())
            {
                T edgeData = auxNode.Data;

                SpanningTree<T> currSP = SpanningTrees[edgeToSpanningTree[edgeData]];
                SpanningTree<T> otherSP = SpanningTrees[OtherIndex(edgeToSpanningTree[edgeData])];

                currSP.RemoveEdge(currSP.DataEdgeMap[edgeData]);
                Trace.Inform($"Removing Edge {DataToString(edgeData)} from Spanning Tree {edgeToSpanningTree[edgeData]}");
                otherSP.AddEdge(graph.DataEdgeMap[edgeData].FromNode.Data, graph.DataEdgeMap[edgeData].ToNode.Data, edgeData);
                edgeToSpanningTree[edgeData] = OtherIndex(edgeToSpanningTree[edgeData]);
                Trace.Inform($"Adding Edge {DataToString(edgeData)} to Spanning Tree {OtherIndex(edgeToSpanningTree[edgeData])}");
            }
            SpanningTrees[startIndex].AddEdge(edge.FromNode.Data, edge.ToNode.Data, edge.Data);
            edgeToSpanningTree[edge.Data] = startIndex;
            Trace.Inform($"Adding Edge {DataToString(edge.Data)} to Spanning Tree {startIndex}");

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

                    // Find a cycle in the other tree, add all of its edges as neighbors of this edge
                    //   in the auxillary graph
                    DFS<t> findCycle = new(strategy.SpanningTrees[currTree])
                    {
                        Start = edge.FromNode.Data,
                        Target = (e) => e.Equals(edge.ToNode.Data)
                    };
                    findCycle.Update();
                    List<Edge<t>> pathEdges = findCycle.GetPathEdges();

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

                            Trace.Debug($"Checking target for {DataToString(edgeData)}");

                            int currTree = edgeToSpanningTree.ContainsKey(edgeData)?
                                        OtherIndex(edgeToSpanningTree[edgeData]):
                                        startWith;

                            IDSU<T> vs = SpanningTrees[currTree].VertexSet;
                            Edge<T> edge = graph.DataEdgeMap[edgeData];
                            bool res = !vs.Find(edge.FromNode.Data).Equals(vs.Find(edge.ToNode.Data));

                            Trace.Debug($"Result : {res}");
                            return res;
                        }
            };
            auxillarySearch.Update();

            return auxillarySearch;
        }
        public virtual void FindSpanningTrees()
        {
            // Initialize spanning trees
            foreach (Node<T> node in graph.Nodes)
            {
                Trace.Inform($"Node : {DataToString(node.Data)}");

                foreach (SpanningTree<T> sp in SpanningTrees)
                {
                    sp.AddNode(node.Data);
                }
            }

            // Growing spanning trees for each edge in graph
            foreach (Edge<T> edge in graph.DataEdgeMap.Values)
            {
                Trace.Debug($"Adding Edge : {DataToString(edge.Data)}");

                TryGrowSpanningTrees(edge);
            }

            ReduceSpanningTrees();

            return;
        }

        public virtual bool SpanningTreesExist()
        {
            foreach (SpanningTree<T> sp in SpanningTrees)
            {
                if (!sp.VertexSet.ContainsElement(source) || !sp.VertexSet.ContainsElement(sink))
                {
                    return false;
                }
                if (!sp.VertexSet.Find(source).Equals(sp.VertexSet.Find(sink)))
                {
                    return false;
                }
            }

            if (spanningTreeA.Edges.Count != spanningTreeB.Edges.Count)
            {
                return false;
            }

            return true;
        }

        public void ReduceSpanningTrees()
        {

            foreach (SpanningTree<T> sp in SpanningTrees)
            {
                foreach (Node<T> node in sp.Nodes)
                {
                    if (!(sp.VertexSet.Find(source).Equals(sp.VertexSet.Find(node.Data)) || !sp.VertexSet.Find(sink).Equals(sp.VertexSet.Find(node.Data))))
                    {
                        // Note, this is not fully correct as the node is not removed from the corresponding DSU
                        // But it functions correctly as the corresponding edges are also removed
                        sp.RemoveNode(node);
                        Trace.Debug($"Reduce : Removing node {node.Data}");
                    }
                }
            }
        }

        public virtual void Short(T edgeData)
        {
            if (shortedEdges.Contains(edgeData))
            {
                throw new GraphException<T>("Edge already shorted", graph);
            }

            if (spanningTreeA.DataEdgeMap.ContainsKey(edgeData) || spanningTreeB.DataEdgeMap.ContainsKey(edgeData))
            {
                shortedEdges.Add(edgeData);
                Edge<T> edge = graph.DataEdgeMap[edgeData];
                SpanningTrees[OtherIndex(edgeToSpanningTree[edgeData])].AddEdge(edge.FromNode.Data, edge.ToNode.Data, edgeData);
            }
            else
            {
                Trace.Warn("Edge not present in either spanningTree");
            }
        }

        public virtual void Cut(T edgeData)
        {
            if (shortedEdges.Contains(edgeData))
            {
                throw new GraphException<T>("Edge already shorted", graph);
            }

            if (spanningTreeA.DataEdgeMap.ContainsKey(edgeData) || spanningTreeB.DataEdgeMap.ContainsKey(edgeData))
            {
                ShortMove = FindShortEdge(edgeToSpanningTree[edgeData] , edgeData);
            }
            else
            {
                Trace.Warn($"Edge {edgeData} not present in either spanningTree");
            }
        }

        private T FindShortEdge(int tree, T cutEdgeData)
        {
            SpanningTree<T> otherST = SpanningTrees[tree]; 
            Edge<T> removedEdge = otherST.DataEdgeMap[cutEdgeData];
            otherST.RemoveEdge(removedEdge);
            
            BFS<T> spanningTreeSearch = new(otherST)
            {
                Start = removedEdge.FromNode.Data,
                Target = (_) => false,
            };
            spanningTreeSearch.Update();

            // This means a corresponding edge has already been shorted
            if (spanningTreeSearch.searchDict.ContainsKey(removedEdge.ToNode.Data))
                return default;

            spanningTreeSearch.Start = removedEdge.ToNode.Data;
            spanningTreeSearch.Update();

            Dictionary<T,T> otherVS = spanningTreeSearch.searchDict;
            
            foreach (Edge<T> e in SpanningTrees[OtherIndex(tree)].Edges)
            {
                if (shortedEdges.Contains(e.Data))
                    continue;

                if (!otherVS.ContainsKey(e.FromNode.Data) || !otherVS.ContainsKey(e.ToNode.Data))
                    continue;

                if (!DSU<T>.CustomFind(e.FromNode.Data, otherVS).Equals(DSU<T>.CustomFind(e.ToNode.Data, otherVS)))
                {
                    return e.Data;
                }
            }
            return default;
        }
    }
}