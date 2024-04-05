using System;
using System.Collections.Generic;

namespace Graphs.Search
{
    public class SimpleSearch<T> : ISearchAlgorithm<T>
    {
        internal Graph<T> searchTree;
        internal Dictionary<T,T> searchDict;
        internal Dictionary<T, Edge<T>> parentEdge;
        internal Func<T, IEnumerable<Edge<T>>> getAdjList = null;


        internal readonly Graph<T> graph;

        public T Start {get; set;}
        public Func<T,bool> Target {get; set;} = (data) => false;
        internal T end = default;

        public bool Reachable {get; internal set;}

        public Graph<T> SearchTree => searchTree;

        public Dictionary<T, T> SearchTreeDict => searchDict;

        public List<Edge<T>> GetPathEdges()
        {
            List<Edge<T>> outList = new();
            T curr = end;
            while (!curr.Equals(Start))
            {
                outList.Add(parentEdge[curr]);
                curr = searchDict[curr];
            }
            outList.Reverse();
            return outList;
        }

        public List<Node<T>> GetPathNodes()
        {
            List<Node<T>> outList = new();
            T curr = end;
            while (!curr.Equals(Start))
            {
                outList.Add(graph.DataNodeMap[curr]);
                curr = searchDict[curr];
            }
            return outList;
        }

        public SimpleSearch(Graph<T> graph)
        {
            this.graph = graph;

            Clear();
        }

        public virtual ISearchCollection<T> GetFrontier() => new SearchQueue<T>();

        public virtual void Clear()
        {
            searchDict = new();
            searchTree = new();
            parentEdge = new();
        }

        public virtual void Update()
        {
            end = Start;


            ISearchCollection<T> frontier = GetFrontier();
            frontier.Add(Start);
            searchTree.AddNode(Start);
            searchDict[Start] = Start;
            parentEdge[Start] = null;

            while (!frontier.IsEmpty())
            {
                T curr = frontier.Pop();

                if (Target(curr))
                {
                    Reachable = true;
                    end = curr;
                    return;
                }

                foreach (Edge<T> edge in graph.DataNodeMap[curr].AdjList)
                {
                    T toData = edge.ToNode.Data;
                    if (!searchDict.ContainsKey(toData))
                    {
                        frontier.Add(toData);
                        searchTree.AddNode(toData);
                        searchTree.AddEdge(curr, toData, edge.Data);
                        searchDict[toData] = curr;
                        parentEdge[toData] = edge;
                    }
                }
            }
        }
    }

    public class DFS<T> : SimpleSearch<T>
    {
        public DFS(Graph<T> graph) : base(graph) {}

        public override ISearchCollection<T> GetFrontier() => new SearchStack<T>();
    }

    public class BFS<T> : SimpleSearch<T>
    {
        public BFS(Graph<T> graph) : base(graph) {}

        public override ISearchCollection<T> GetFrontier() => new SearchQueue<T>();
    }
}