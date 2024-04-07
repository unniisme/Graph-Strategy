using System.Collections.Generic;
using Gamelogic.Managers;
using Graphs.Utils;
namespace Graphs
{
    public class Node<T>
    {
        public virtual T Data { get; set; }
        public virtual List<Edge<T>> AdjList { get; } = new List<Edge<T>>();
        public virtual int Degree => AdjList.Count;
        public virtual List<Node<T>> Neighbors => AdjList.ConvertAll((Edge<T> edge) => edge.ToNode);
    }

    public class Edge<T>
    {
        public Node<T> FromNode { get; set; }
        public Node<T> ToNode { get; set; }
        public T Data { get; set; }
    }

    /// <summary>
    /// Multigraph
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Graph<T>
    {
        public int UID {get;} = GameResources.UIDgen;

        public HashSet<Node<T>> Nodes { get; } = new HashSet<Node<T>>();
        public HashSet<Edge<T>> Edges { get; } = new HashSet<Edge<T>>();

        public Dictionary<T, Node<T>> DataNodeMap = new();

        /// <summary>
        /// Data to edge map
        /// Consistency has to be maintained externally
        /// </summary>
        public Dictionary<T, Edge<T>> DataEdgeMap = new();

        public virtual Node<T> AddNode(T data)
        {
            Node<T> newNode = new() { Data = data };
            Nodes.Add(newNode);

            if (DataNodeMap.ContainsKey(data))
                throw new GraphException<T>("Graph nodes cannot contain duplicate keys", this);

            DataNodeMap[data] = newNode;
            return newNode;
        }

        public virtual void RemoveNode(Node<T> node)
        {
            List<Edge<T>> adjList = new (node.AdjList);
            foreach (Edge<T> edge in adjList)
            {
                RemoveEdge(edge);
            }
            Nodes.Remove(node);
            DataNodeMap.Remove(node.Data);
        }

        public virtual void AddEdge(Node<T> fromNode, Node<T> toNode, T data)
        {
            Edge<T> newEdge = new()
            {
                FromNode = fromNode,
                ToNode = toNode,
                Data = data
            };

            // Maintain edge map consistency outside instead
            // if (DataEdgeMap.ContainsKey(data))
            //     throw new GraphException<T>("Graph edges cannot contain duplicate keys", this);

            DataEdgeMap[data] = newEdge;

            fromNode.AdjList.Add(newEdge);
            Edges.Add(newEdge);
        }

        public virtual void AddEdge(T fromData, T toData, T edgeData)
        {
            AddEdge(DataNodeMap[fromData], DataNodeMap[toData], edgeData);
        }

        public virtual void RemoveEdge(Edge<T> edge)
        {
            edge.FromNode.AdjList.Remove(edge);
            Edges.Remove(edge);
            DataEdgeMap.Remove(edge.Data);
        }

        public Edge<T> GetEdge(Node<T> from, Node<T> to)
        {
            // Possibly replace with an adjmat
            foreach (Edge<T> edge in from.AdjList)
            {
                if (edge.ToNode == to)
                {
                    return edge;
                }
            }
            return null;
        }
    }
}