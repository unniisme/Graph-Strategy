using System;
using System.Collections.Generic;

namespace Graphs.Search
{
    public interface ISearchAlgorithm<T>
    {
        /// <summary>
        /// Start of search
        /// </summary>
        public T Start {get; set;}

        /// <summary>
        /// Target function to end search.
        /// Leave always false to search entire graph
        /// </summary>
        public Func<T,bool> Target {get;set;}

        /// <summary>
        /// Whether the target is reachable or not
        /// false if target is null
        /// </summary>
        public bool Reachable {get;}

        /// <summary>
        /// Directed graph representing search tree.
        /// Directed outwards from Start
        /// </summary>
        public Graph<T> SearchTree {get;}

        /// <summary>
        /// Search tree in dictionary format, rooted at Start
        /// </summary>
        public Dictionary<T,T> SearchTreeDict {get;}

        /// <summary>
        /// List of edges that lead from Start to Target
        /// 
        /// Returns empty if target is null or unreachable
        /// </summary>
        /// <returns></returns>
        public List<Edge<T>> GetPathEdges();

        /// <summary>
        /// List of nodes that lead from Target to Start
        /// 
        /// Returns empty if target is null or unreachable
        /// </summary>
        /// <returns></returns>
        public List<Node<T>> GetPathNodes();

        public Func<T, IEnumerable<Edge<T>>> GetAdjList {get;set;}
    }
}