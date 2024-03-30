using System;
using System.Collections.Generic;

namespace Graphs.Shannon
{
    public interface IShannonStrategy<T>
    {
        public Tuple<List<Edge<T>>, List<Edge<T>>> GetSpanningTrees();
        
    }
}