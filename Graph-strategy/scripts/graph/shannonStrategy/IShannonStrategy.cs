namespace Graphs.Shannon
{
    public interface IShannonStrategy<T>
    {
        public void FindSpanningTrees();

        public bool SpanningTreesExist(T a, T b);
        
        public void Cut(T edgeData);
        public void Short(T edgeData);

        public T GetShortMove();
    }
}