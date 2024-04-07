using Godot;
using Graphs;

namespace Gamelogic.Grid.Graph
{
    public class DualGridGraph : NavigationGridGraph
    {
        private Graph<Islet<Vector2I>> islandGraph = null;
        public DualGridGraph(IGrid grid) : base(grid) {}
        
        public void SearchAll()
        {
            foreach (IGridObject gridObject in grid.PlacedObjects)
            {
                MakeNode(gridObject.GridPosition);
            }
        }

        public DualGridGraph(IGrid grid, Graph<Islet<Vector2I>> islandGraph) : this(grid)
        {
            this.islandGraph = islandGraph;
            SearchAll();
        }

        public DualGridGraph(IGrid grid, Vector2I start) : base(grid, start) {}

        internal override bool CheckPos(Vector2I pos)
        {
            if (islandGraph != null)
            {
                // Inefficient but fine since it's one time
                foreach (Edge<Islet<Vector2I>> edge in islandGraph.Edges)
                {
                    if (edge.Data.Contains(pos))
                    {
                        return true;
                    }
                }

            }

            return grid.GetObject(pos) != null;
        }
    }
}