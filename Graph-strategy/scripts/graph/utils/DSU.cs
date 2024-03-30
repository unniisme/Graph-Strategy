using System;
using System.Collections.Generic;

namespace Graphs.Utils
{
    public class DSU<T>
    {
        internal readonly Dictionary<T, T> parent;
        internal readonly Dictionary<T, int> rank;

        public DSU()
        {
            parent = new Dictionary<T, T>();
            rank = new Dictionary<T, int>();
        }

        public bool ContainsElement(T element)
        {
            return parent.ContainsKey(element);
        }

        public void MakeSet(T element)
        {
            if (!parent.ContainsKey(element))
            {
                parent[element] = element;
                rank[element] = 0;
            }
        }

        public T Find(T element)
        {
            if (!parent.ContainsKey(element))
            {
                throw new ArgumentException("Element does not exist in the DSU.");
            }

            if (!parent[element].Equals(element))
            {
                parent[element] = Find(parent[element]);
            }

            return parent[element];
        }

        public void Union(T x, T y)
        {
            T rootX = Find(x);
            T rootY = Find(y);

            if (rootX.Equals(rootY))
            {
                return; // Already in the same set
            }

            if (rank[rootX] < rank[rootY])
            {
                parent[rootX] = rootY;
            }
            else if (rank[rootX] > rank[rootY])
            {
                parent[rootY] = rootX;
            }
            else
            {
                parent[rootY] = rootX;
                rank[rootX]++;
            }
        }
    }
}
