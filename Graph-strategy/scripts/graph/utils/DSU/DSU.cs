using System;
using System.Collections.Generic;

namespace Graphs.Utils
{
    public class DSU<T> : IDSU<T>
    {
        internal readonly Dictionary<T, T> parent = new();
        internal readonly Dictionary<T, int> rank = new();

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

        public static T CustomFind(T element, Dictionary<T,T> parentDict)
        {
            if (!parentDict.ContainsKey(element))
            {
                throw new ArgumentException($"Element {element} does not exist in the DSU.");
            }

            if (!parentDict[element].Equals(element))
            {
                parentDict[element] = CustomFind(parentDict[element], parentDict);
            }

            return parentDict[element];
        }

        public T Find(T element)
        {
            if (!parent.ContainsKey(element))
            {
                throw new ArgumentException($"Element {element} does not exist in the DSU.");
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
