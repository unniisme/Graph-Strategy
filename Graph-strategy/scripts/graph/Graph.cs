using System.Collections.Generic;
namespace Graphs
{
    public class Node<T> : INode<T>
    {
        public T Data { get; set; }
        public virtual List<IEdge<T>> AdjList { get; } = new List<IEdge<T>>();
        public int Degree => AdjList.Count;
        public List<Node<T>> Neighbors => AdjList.ConvertAll((IEdge<T> edge) => edge.ToNode);
    }

    public class Edge<T> : IEdge<T>
    {
        public Node<T> FromNode { get; set; }
        public Node<T> ToNode { get; set; }
        public T Data { get; set; }
    }

    public class Graph<T> : IGraph<T>
    {
        public List<Node<T>> Nodes { get; } = new List<Node<T>>();
        public List<Edge<T>> Edges { get; } = new List<Edge<T>>();

        List<INode<T>> IGraph<T>.Nodes => throw new System.NotImplementedException();

        List<IEdge<T>> IGraph<T>.Edges => throw new System.NotImplementedException();

        public Dictionary<Node<T>, Dictionary<Node<T>, Edge<T>>> AdjMat = new();

        public Dictionary<T, Node<T>> DataNodeMap = new();

        public virtual Node<T> AddNode(T data)
        {
            Node<T> newNode = new() { Data = data };
            Nodes.Add(newNode);

            AdjMat[newNode] = new() {};
            foreach (Node<T> node in Nodes)
            {
                AdjMat[newNode][node] = null;
            }

            DataNodeMap[data] = newNode;
            return newNode;
        }

        public virtual void RemoveNode(Node<T> node)
        {
            foreach (Edge<T> edge in node.AdjList)
            {
                RemoveEdge(edge);
            }
            foreach (Node<T> fromNode in AdjMat.Keys)
            {
                if (AdjMat[fromNode][node] != null)
                {
                    RemoveEdge(AdjMat[fromNode][node]);
                }
            }
            AdjMat.Remove(node);
            foreach (Dictionary<Node<T>, Edge<T>> row in AdjMat.Values)
            {
                row.Remove(node);
            }
            Nodes.Remove(node);
            DataNodeMap.Remove(node.Data);
        }

        public virtual void AddEdge(Node<T> fromNode, Node<T> toNode, T data)
        {
            if (AdjMat[fromNode][toNode] != null)
            {
                throw new GraphException<T>("Edge already present", this);
            }

            Edge<T> newEdge = new()
            {
                FromNode = fromNode,
                ToNode = toNode,
                Data = data
            };
            fromNode.AdjList.Add(newEdge);
            Edges.Add(newEdge);
            AdjMat[fromNode][toNode] = newEdge;
        }

        public void AddEdge(T fromData, T toData, T edgeData)
        {
            AddEdge(DataNodeMap[fromData], DataNodeMap[toData], edgeData);
        }

        public virtual void RemoveEdge(Node<T> fromNode, Node<T> toNode)
        {
            if (AdjMat[fromNode][toNode] == null)
            {
                throw new GraphException<T>("Edge not present", this);
            }

            Edge<T> edge = AdjMat[fromNode][toNode];
            fromNode.AdjList.Remove(edge);
            Edges.Remove(edge);
            AdjMat[fromNode][toNode] = null;
        }

        public virtual void RemoveEdge(Edge<T> edge)
        {
            RemoveEdge(edge.FromNode, edge.ToNode);
        }
    }

    /// <summary>
    /// Undirected graph
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleGraph<T> : Graph<T>
    {
        public override void AddEdge(Node<T> fromNode, Node<T> toNode, T data)
        {
            base.AddEdge(fromNode, toNode, data);
            base.AddEdge(fromNode, toNode, data);
        }

        public override void RemoveEdge(Node<T> fromNode, Node<T> toNode)
        {
            base.RemoveEdge(fromNode, toNode);
            base.RemoveEdge(toNode, fromNode);
        }

        public override void RemoveEdge(Edge<T> edge)
        {
            RemoveEdge(edge.FromNode, edge.ToNode);
            RemoveEdge(edge.ToNode, edge.FromNode);
        }
    }
}