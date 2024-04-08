using System.Collections.Generic;
using Graphs.Utils;

namespace Graphs
{
    /// <summary>
    /// Class represents either an island - a well connected collection of nodes
    ///     or a bridge - a chain of degree 2 edges
    /// </summary>
    public class Islet<T> : DSU<T>
    {   
        private readonly HashSet<T> islands = new();
        public bool IsIsland(T t) => islands.Contains(Find(t));
        public void MakeSet(T value, bool isIsland)
        {
            MakeSet(value);
            if (isIsland) islands.Add(value);
        }

        // Union such that final set gets the island status of the left
        public void LeftUnion(T a, T b)
        {
            bool islandStatus = IsIsland(a);

            Union(a,b);

            if (IsIsland(Find(a)) != islandStatus)
            {
                if (islandStatus)
                {
                    islands.Add(Find(a));
                }
                else
                {
                    islands.Remove(Find(a));
                }
            }
        }
    }
}