using System.Collections.Generic;
using Graphs.Search;
using Graphs.Utils;
using Logging;

namespace Graphs.Shannon
{
    public class GraphUpdateShannonStrategy<T> : TwoPlayerShannonStrategy<T>
    {
        public Logger Logger {get;set;} = new Logger("GraphUpdateShannonStrategy");

        public GraphUpdateShannonStrategy(Graph<T> graph) : base(graph) {}

        public override void Cut(T edgeData)
        {
            if (!graph.DataEdgeMap.ContainsKey(edgeData)) return;

            Logger?.Inform($"Cutting {edgeData} ({graph.DataEdgeMap[edgeData].FromNode.Data}) -> {graph.DataEdgeMap[edgeData].ToNode.Data}");

            graph.RemoveEdge(graph.DataEdgeMap[edgeData]);
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
            Logger?.Warn("Edge not present in either spanningTree");
        }

        public override void Short(T edgeData)
        {
            if (!graph.DataEdgeMap.ContainsKey(edgeData)) return;

            Logger?.Inform($"Shorting {edgeData} ({graph.DataEdgeMap[edgeData].FromNode.Data}) -> {graph.DataEdgeMap[edgeData].ToNode.Data}");

            Edge<T> shortedEdge = graph.DataEdgeMap[edgeData];
            graph.ShortEdge(shortedEdge);

            ShortMove = default;

            Clear();
            FindSpanningTrees();
        }
        private T CutEdge(int tree, T cutEdgeData)
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
            
            foreach (Edge<T> e in SpanningTrees[tree].Edges)
            {
                if (!DSU<T>.CustomFind(e.FromNode.Data, otherVS).Equals(DSU<T>.CustomFind(e.ToNode.Data, otherVS)))
                {
                    otherST.AddEdge(e.FromNode.Data, e.ToNode.Data, cutEdgeData);
                    return e.Data;
                }
            }
            return default;
        }
    }
}