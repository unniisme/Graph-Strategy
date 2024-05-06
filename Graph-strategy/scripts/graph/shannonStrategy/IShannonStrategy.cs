using System.Collections.Generic;
using Graphs.Utils;

namespace Graphs.Shannon
{
    public interface IShannonStrategy<T>
    {
        public void FindSpanningTrees();

        public bool SpanningTreesExist();

        public List<SpanningTree<T>> SpanningTrees {get;}
        
        public void Cut(T edgeData);
        public void Short(T edgeData);

        public T ShortMove {get;}

        public void Clear();
    }
}