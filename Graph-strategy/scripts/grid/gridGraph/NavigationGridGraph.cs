using Godot;
using Graphs;

namespace Gamelogic.Grid
{
    public class NavigationGridGraph : Graph<Vector2I>
    {
        private readonly Vector2I[] dirs = {
            Vector2I.Up,
            Vector2I.Left,
            Vector2I.Down,
            Vector2I.Right
        };

        private readonly IGrid grid;
        public NavigationGridGraph(IGrid grid, Vector2I startPos)
        {
            this.grid = grid;

            MakeNode(startPos);
        }

        private Node<Vector2I> MakeNode(Vector2I pos)
        {

            Node<Vector2I> fromNode = AddNode(pos);

            foreach (Vector2I dir in dirs)
            {
                Vector2I newPos =  grid.GetPositionInDirection(pos, dir);
                
                if (grid.GetObject(newPos) != null) continue;
                if (DataNodeMap.ContainsKey(newPos))
                {
                    if (!fromNode.Neighbors.Contains(DataNodeMap[newPos]))
                        AddEdge(fromNode, DataNodeMap[newPos], pos+newPos);
                    continue;
                }

                Node<Vector2I> toNode = MakeNode(newPos);

                AddEdge(fromNode, toNode, pos+newPos);
            }

            return fromNode;
        }
    }
}