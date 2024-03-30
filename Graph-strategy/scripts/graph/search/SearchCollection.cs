using System.Collections.Generic;

namespace Graphs.Search
{
    public interface ISearchCollection<T>
    {
        /// <summary>
        /// Add a new element to collection
        /// </summary>
        /// <param name="element"></param>
        public void Add(T element);

        /// <summary>
        /// Remove the highest priorit element from collection
        /// </summary>
        /// <returns></returns>
        public T Pop();

        /// <summary>
        /// Whether the collection is empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty();
    }

    public class SearchQueue<T> : Queue<T>, ISearchCollection<T>
    {
        public void Add(T element) => Enqueue(element);
        public bool IsEmpty() => Count == 0;
        public T Pop() => Dequeue();
    }

    public class SearchStack<T> : Stack<T>, ISearchCollection<T>
    {
        public void Add(T elemet) => Push(elemet);
        public bool IsEmpty() => Count == 0;
    }
}