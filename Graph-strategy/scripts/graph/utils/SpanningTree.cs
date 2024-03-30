namespace Graphs.Utils
{
    /// <summary>
    /// Graph that also maintains an edge set
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

        public override void AddEdge(Node<T> fromNode, Node<T> toNode, T data)
        {
            // Note, while the graph itself isn't undirected, the DSU assumes so
            VertexSet.Union(fromNode.Data, toNode.Data);
            base.AddEdge(fromNode, toNode, data);
        }

        public override void RemoveEdge(Edge<T> edge)
        {
            if (VertexSet.parent[edge.FromNode.Data].Equals(edge.ToNode.Data))
            {
                VertexSet.parent[edge.FromNode.Data] = edge.FromNode.Data; 
            }
            else
            {
                VertexSet.parent[edge.ToNode.Data] = edge.ToNode.Data; 
            }
            base.RemoveEdge(edge);
        }

        public override void RemoveNode(Node<T> node)
        {
            VertexSet.parent.Remove(node.Data);
            VertexSet.rank.Remove(node.Data);

            base.RemoveNode(node);
        }
    }
}