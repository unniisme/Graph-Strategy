using System;
using System.Collections.Generic;

namespace Graphs.Search
{
    public class SimpleSearch<T> : ISearchAlgorithm<T>
    {
        private Graph<T> searchTree;
        private Dictionary<T,T> searchDict;
        private Dictionary<T, Edge<T>> parentEdge;
        private Func<T, IEnumerable<Edge<T>>> getAdjList = null;


        private readonly Graph<T> graph;

        public T Start {get; set;}
        public Func<T,bool> Target {get; set;} = (data) => false;
        private T end = default;

        public bool Reachable {get; private set;}

        public Graph<T> SearchTree => searchTree;

        public Dictionary<T, T> SearchTreeDict => searchDict;

        public Func<T, IEnumerable<Edge<T>>> GetAdjList 
        {
            set => getAdjList = value;
            get
            {
                return getAdjList??((data) => graph.DataNodeMap[data].AdjList);
            }
        }
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
        }

        public virtual ISearchCollection<T> GetFrontier() => new SearchQueue<T>();

        public void Update()
        {
            searchDict = new();
            searchTree = new();
            parentEdge = new();

            end = Start;

            foreach (Node<T> node in graph.Nodes)
            {
                searchTree.AddNode(node.Data);
                searchDict[node.Data] = default;
            }

            ISearchCollection<T> frontier = GetFrontier();
            frontier.Add(Start);

            while (!frontier.IsEmpty())
            {
                T curr = frontier.Pop();

                if (Target(curr))
                {
                    Reachable = true;
                    end = curr;
                    return;
                }

                if (searchDict.ContainsKey(curr)) continue;

                searchTree.AddNode(curr);
                searchDict[curr] = default;
                parentEdge[curr] = null;

                foreach (Edge<T> edge in GetAdjList(curr))
                {
                    T toData = edge.ToNode.Data;
                    if (!searchDict.ContainsKey(toData))
                    {
                        frontier.Add(toData);
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