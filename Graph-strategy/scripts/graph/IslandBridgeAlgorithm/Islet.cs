namespace Graphs
{
    /// <summary>
    /// Class represents either an island - a well connected collection of nodes
    ///     or a bridge - a chain of degree 2 edges
    /// </summary>
    public class Islet<T> : Graph<T>
    {
        public bool IsIsland; // Bridge if false

        
        public Islet(bool isIsland)
        {
            IsIsland = isIsland;
        }

        public override void AddEdge(Node<T> fromNode, Node<T> toNode, T data)
        {
            base.AddEdge(fromNode, toNode, data);

            if (fromNode.AdjList.Count > 2 && !IsIsland)
                throw new GraphInconsistencyException<T>("Bridge can't have degree > 2 vertices", this);
        }

        public void AddNode(Node<T> newNode)
        {
            if (Nodes.Contains(newNode))
                return;
                // throw new GraphException<T>("Islet already has edge", this);

            Nodes.Add(newNode);
        }

        public void JoinNodes(Islet<T> other)
        {
            foreach (Node<T> node in other.Nodes)
            {
                AddNode(node);
            }
        }
    }
}