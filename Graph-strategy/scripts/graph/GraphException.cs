using System;

namespace Graphs
{
    public class GraphException<T> : Exception
    {
        public Graph<T> graph;

        public GraphException(string message, Graph<T> obj)
            : base($"Graph Exception of {obj} : {message}")
        {
            graph = obj;
        }
    }

    public class GraphInconsistencyException<T> : GraphException<T>
    {
        public GraphInconsistencyException(string message, Graph<T> obj)
            : base($"Graph inconsistency at {obj} : {message}", obj)
        {
            graph = obj;
        }
    }
}