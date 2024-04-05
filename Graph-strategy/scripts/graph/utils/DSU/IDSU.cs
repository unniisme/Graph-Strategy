namespace Graphs.Utils
{
    public interface IDSU<T>
    {
        public bool ContainsElement(T element);
        public void MakeSet(T element);
        public T Find(T element);
        public void Union(T x, T y);

    }
}