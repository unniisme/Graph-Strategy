namespace Graphs.Utils
{
    /// <summary>
    /// Undirected Graph that also maintains an edge set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SpanningTree<T> : Graph<T>
    {
        public DSU<T> VertexSet {get;set;} = new();

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
            if (VertexSet.parent[edge.FromNode.Data].Equals(edge.ToNode.Data))
            {
                VertexSet.parent[edge.FromNode.Data] = edge.FromNode.Data; 
            }
            else if (VertexSet.parent[edge.ToNode.Data].Equals(edge.FromNode.Data))
            {
                VertexSet.parent[edge.ToNode.Data] = edge.ToNode.Data; 
            }
            base.RemoveEdge(edge);

            // Remove backedge, as unweightedness is simulated by a bygraph
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
            VertexSet.parent.Remove(node.Data);
            VertexSet.rank.Remove(node.Data);

            base.RemoveNode(node);
        }
    }
}