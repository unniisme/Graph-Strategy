using System.Collections.Generic;

namespace Graphs
{
    public interface INode<T>
    {
        public T Data { get; set; }
        public int Degree {get;}
        public List<INode<T>> Neighbors {get;}
    }

    public interface IEdge<T>
    {
        public INode<T> FromNode { get; set; }
        public INode<T> ToNode { get; set; }
        public T Data { get; set; }
    }

    public interface IGraph<T>
    {
        public List<INode<T>> Nodes { get; }
        public List<IEdge<T>> Edges { get; }
    }
}