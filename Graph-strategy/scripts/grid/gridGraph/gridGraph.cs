using Godot;
using Graphs;

namespace Gamelogic.Grid
{
    public class GridCell : Node<Node2D>
    {
        public Vector2I Position;
        public IGridObject Object;
    }

    public class GridGraph : Graph<GridCell>
    {

    }
}