using Godot;
using Graphs;

namespace Gamelogic.Grid.Graph
{
    public class NavigationGridGraph : Graph<Vector2I>
    {
        public readonly Vector2I[] dirs = {
            Vector2I.Up,
            Vector2I.Left,
            Vector2I.Down,
            Vector2I.Right
        };

        internal readonly IGrid grid;

        public NavigationGridGraph(IGrid grid)
        {
            this.grid = grid;
        }
        public NavigationGridGraph(IGrid grid, Vector2I startPos)
        {
            this.grid = grid;

            MakeNode(startPos);
        }

        internal Node<Vector2I> MakeNode(Vector2I pos)
        {

            if (DataNodeMap.ContainsKey(pos))
                return DataNodeMap[pos];

            Node<Vector2I> fromNode = AddNode(pos);

            foreach (Vector2I dir in dirs)
            {
                Vector2I newPos =  grid.GetPositionInDirection(pos, dir);
                
                if (!CheckPos(newPos)) continue;
                if (DataNodeMap.ContainsKey(newPos))
                {
                    if (!fromNode.Neighbors.Contains(DataNodeMap[newPos]))
                        AddEdge(fromNode, DataNodeMap[newPos], pos+newPos + 100003 * (newPos - pos));
                    continue;
                }

                Node<Vector2I> toNode = MakeNode(newPos);

                // The weird data equation is to preserve uniqueness of edges
                // Each edge is a pseudo independant pair of vectors 
                // As long as coordinates are less than 1000003
                AddEdge(fromNode, toNode, pos+newPos + 1000003 * (newPos - pos));
            }

            return fromNode;
        }

        internal virtual bool CheckPos(Vector2I pos)
        {
            return grid.GetObject(pos) == null;
        }
    }
}