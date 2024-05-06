using System.Collections.Generic;
using System.Linq;
using Graphs.Utils;
using Logging;

namespace Graphs.Shannon
{
    public class WeightedData<T>
    {
        public T item;
        public int weight;

        public static WeightedData<T> Node(T data)
        {
            return new WeightedData<T>() 
            {
                item = data,
                weight = -1
            };
        }

        public static WeightedData<T> Edge(T data, int weight)
        {
            return new WeightedData<T>() 
            {
                item = data,
                weight = weight
            };
        }

        public override string ToString()
        {
            return $"{item} | weight = {weight}";
        }

        public override bool Equals(object obj)
        {
            if (obj is WeightedData<T> other)
            {
                return item.Equals(other.item) && weight == other.weight;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return 10007 * item.GetHashCode() + weight;
        }
    }

    public class WeightedShannonStrategy<T> : GraphUpdateShannonStrategy<WeightedData<T>>
    {

        public override Logger Trace { get; set; } = new Logger("WeightedShannonStrategy");
        public WeightedShannonStrategy(IslandBridgeGraph<T> graph, T source, T sink) : base()
        {
            IslandBridgeGraph<WeightedData<T>> weightedGraph = new ()
            {
                Trace = new Logger(graph.Trace, "Weighted"),
            };

            // Construct a weighted graph by duplicating each edge and doubling the weight of the second edge

            foreach (Node<T> node in graph.Nodes)
            {
                var nodeData = WeightedData<T>.Node(node.Data);
                weightedGraph.AddNode(nodeData);
                weightedGraph.islets.MakeSet(nodeData);
                if (weightedGraph.islets.ContainsElement(WeightedData<T>.Node(graph.islets.Find(node.Data))))
                {
                    weightedGraph.islets.LeftUnion(WeightedData<T>.Node(graph.islets.Find(node.Data)), nodeData);
                }
            }
            foreach (Edge<T> edge in graph.Edges)
            {
                weightedGraph.AddEdge(WeightedData<T>.Node(edge.FromNode.Data), 
                                        WeightedData<T>.Node(edge.ToNode.Data),
                                        WeightedData<T>.Edge(edge.Data, 1));
                weightedGraph.AddEdge(WeightedData<T>.Node(edge.FromNode.Data), 
                                        WeightedData<T>.Node(edge.ToNode.Data),
                                        WeightedData<T>.Edge(edge.Data, 2));
            }

            this.graph = weightedGraph;
            this.source = WeightedData<T>.Node(source);
            this.sink = WeightedData<T>.Node(sink);

            Clear();

            Trace.Inform("Initializing");
        }


        public override void FindSpanningTrees()
        {
            // Initialize spanning trees
            foreach (Node<WeightedData<T>> node in graph.Nodes)
            {
                Trace.Inform($"Node : {DataToString(node.Data)}");

                foreach (SpanningTree<WeightedData<T>> sp in SpanningTrees)
                {
                    sp.AddNode(node.Data);
                }
            }

            List<Edge<WeightedData<T>>> orderedEdges = graph.Edges.ToList();
            orderedEdges.Sort(Comparer<Edge<WeightedData<T>>>.Create((x, y) => x.Data.weight.CompareTo(y.Data.weight)));

            // Growing spanning trees for each edge in graph
            foreach (Edge<WeightedData<T>> edge in orderedEdges)
            {
                Trace.Debug($"Adding Edge : {DataToString(edge.Data)}");

                bool result = TryGrowSpanningTrees(edge);

                if (result && edge.Data.weight > 1)
                {
                    ShortMove = edge.Data;
                }
            }

            ReduceSpanningTrees();

            return;
        }

        public override bool SpanningTreesExist()
        {
            if (base.SpanningTreesExist())
            {
                foreach (WeightedData<T> edge in edgeToSpanningTree.Keys)
                {
                    if (edge.weight > 1)
                        return false;
                }
                return true;
            }
            return false;
        }
    }
}