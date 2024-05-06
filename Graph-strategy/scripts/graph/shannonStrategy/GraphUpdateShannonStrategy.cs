using System.Collections.Generic;
using Graphs.Search;
using Graphs.Utils;
using Logging;

namespace Graphs.Shannon
{
    public class GraphUpdateShannonStrategy<T> : TwoPlayerShannonStrategy<T>
    {
        public override Logger Trace {get;set;} = new Logger("GraphUpdateShannonStrategy");

        public GraphUpdateShannonStrategy(Graph<T> graph, T source, T sink) : base(graph, source, sink) {}
        public GraphUpdateShannonStrategy() : base() {}

        public override void Cut(T edgeData)
        {
            if (!graph.DataEdgeMap.ContainsKey(edgeData))
            {
                Trace?.Error($"(Cutting) Edge {edgeData} not present in graph");
                return;
            }

            Trace?.Inform($"Cutting {edgeData} ({graph.DataEdgeMap[edgeData].FromNode.Data}) -> {graph.DataEdgeMap[edgeData].ToNode.Data}");

            graph.RemoveEdge(graph.DataEdgeMap[edgeData]);

            if (SpanningTreesExist())
            {
                if (spanningTreeA.DataEdgeMap.ContainsKey(edgeData))
                {
                    ShortMove = CutEdge(1, edgeData);
                    return;
                }
                else if (spanningTreeB.DataEdgeMap.ContainsKey(edgeData))
                {
                    ShortMove = CutEdge(0, edgeData);
                    return;
                }
            }
            Trace?.Warn("Edge not present in either spanningTree");
        }

        public override void Short(T edgeData)
        {
            if (!graph.DataEdgeMap.ContainsKey(edgeData))
            {
                Trace?.Error($"(Shorting) Edge {edgeData} not present in graph");
                return;
            }

            Trace?.Inform($"Shorting {edgeData} ({graph.DataEdgeMap[edgeData].FromNode.Data}) -> {graph.DataEdgeMap[edgeData].ToNode.Data}");

            Edge<T> shortedEdge = graph.DataEdgeMap[edgeData];
            Node<T> mergedNode = graph.ShortEdge(shortedEdge);

            if (source.Equals(shortedEdge.ToNode.Data) || source.Equals(shortedEdge.FromNode.Data)) source = mergedNode.Data;
            if (sink.Equals(shortedEdge.ToNode.Data) || sink.Equals(shortedEdge.FromNode.Data)) sink = mergedNode.Data;

            ShortMove = default;

            // Do outside from manager
            // Clear();
            // FindSpanningTrees();
        }
        internal T CutEdge(int tree, T cutEdgeData)
        {
            SpanningTree<T> otherST = SpanningTrees[OtherIndex(tree)]; 
            Edge<T> removedEdge = otherST.DataEdgeMap[cutEdgeData];
            otherST.RemoveEdge(otherST.DataEdgeMap[cutEdgeData]);
            
            BFS<T> spanningTreeSearch = new(otherST)
            {
                Start = removedEdge.FromNode.Data,
                Target = (_) => false,
            };
            spanningTreeSearch.Update();
            spanningTreeSearch.Start = removedEdge.ToNode.Data;
            spanningTreeSearch.Update();

            Dictionary<T,T> otherVS = spanningTreeSearch.searchDict;

            otherST.AddEdge(removedEdge.FromNode.Data, removedEdge.ToNode.Data, cutEdgeData);
            
            foreach (Edge<T> e in SpanningTrees[tree].Edges)
            {
                if (!DSU<T>.CustomFind(e.FromNode.Data, otherVS).Equals(DSU<T>.CustomFind(e.ToNode.Data, otherVS)))
                {
                    return e.Data;
                }
            }
            Trace.Warn("No move found even though spanning trees exist");
            
            return default;
        }
    }
}