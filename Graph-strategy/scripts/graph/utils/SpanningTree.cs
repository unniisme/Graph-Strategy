using System.Collections.Generic;
using Logging;

namespace Graphs.Utils
{
    /// <summary>
    /// Undirected Graph that also maintains an edge set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SpanningTree<T> : Graph<T>
    {
        public DSU<T> VertexSet {get;} = new();

        public override Node<T> AddNode(T data)
        {
            VertexSet.MakeSet(data);
            Node<T> node = base.AddNode(data);
            return node;
        }

        public override void AddEdge(T fromData, T toData, T data)
        {
            // Note, while the graph itself isn't undirected, the DSU assumes so
            VertexSet.Union(fromData, toData);
            base.AddEdge(toData, fromData, data);
            base.AddEdge(fromData, toData, data);
        }

        public override void RemoveEdge(Edge<T> edge)
        {
            base.RemoveEdge(edge);

            // Remove backedge, as undirectedness is simulated by a bygraph
            foreach (Edge<T> e in edge.ToNode.AdjList.ToArray())
            {
                if (e.Data.Equals(edge.Data) && e.ToNode.Data.Equals(edge.FromNode.Data))
                {
                    base.RemoveEdge(e);
                }
            }
        }

        public override void RemoveNode(Node<T> node)
        {
            foreach (Edge<T> edge in node.AdjList.ToArray())
            {
                if (DataEdgeMap.ContainsKey(edge.Data))
                    RemoveEdge(DataEdgeMap[edge.Data]);
            }
            Nodes.Remove(node);
            DataNodeMap.Remove(node.Data);

            // VertexSet.parent.Remove(node.Data); // Should do more deliberation on this one
        }
        
    }
}