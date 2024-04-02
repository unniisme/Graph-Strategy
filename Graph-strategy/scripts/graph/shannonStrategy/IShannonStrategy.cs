using System;
using System.Collections.Generic;

namespace Graphs.Shannon
{
    public interface IShannonStrategy<T>
    {
        public Tuple<List<Edge<T>>, List<Edge<T>>> GetSpanningTrees(T a, T b);

        public bool SpanningTreesExist {get;}
        
        public void Cut(T edgeData);
        public void Short(T edgeData);

        public T GetShortMove();
    }
}