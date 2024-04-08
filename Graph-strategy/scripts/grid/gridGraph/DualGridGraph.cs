using Godot;
using Graphs;

namespace Gamelogic.Grid.Graph
{
    public class DualGridGraph : NavigationGridGraph
    {
        private readonly IslandBridgeGraph<Vector2I> islandGraph = null;
        public DualGridGraph(IGrid grid) : base(grid) {}
        
        public void SearchAll()
        {
            foreach (IGridObject gridObject in grid.PlacedObjects)
            {
                MakeNode(gridObject.GridPosition);
            }
        }

        public DualGridGraph(IGrid grid, IslandBridgeGraph<Vector2I> islandGraph) : this(grid)
        {
            this.islandGraph = islandGraph;
            SearchAll();
        }

        public DualGridGraph(IGrid grid, Vector2I start) : base(grid, start) {}

        internal override bool CheckPos(Vector2I pos)
        {
            if (islandGraph != null)
            {
                if (islandGraph.islets.ContainsElement(pos))
                {
                    if (!islandGraph.islets.IsIsland(pos))
                        return true;
                }
            }

            return grid.GetObject(pos) != null;
        }
    }
}