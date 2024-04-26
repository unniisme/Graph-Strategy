namespace Graphs.Shannon
{
    public interface IShannonStrategy<T>
    {
        public void FindSpanningTrees();

        public bool SpanningTreesExist();
        
        public void Cut(T edgeData);
        public void Short(T edgeData);

        public T ShortMove {get;}
    }
}