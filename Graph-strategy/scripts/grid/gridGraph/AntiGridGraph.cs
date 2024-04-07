using Godot;

namespace Gamelogic.Grid.Graph
{
    public class AntiGridGraph : NavigationGridGraph
    {
        public AntiGridGraph(IGrid grid) : base(grid) 
        {
            foreach (IGridObject gridObject in grid.PlacedObjects)
            {
                MakeNode(gridObject.GridPosition);
            }
        }

        internal override bool CheckPos(Vector2I pos)
        {
            return grid.GetObject(pos) != null;
        }
    }
}