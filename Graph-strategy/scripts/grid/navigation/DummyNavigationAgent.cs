using Godot;

namespace Gamelogic.Grid
{
    /// <summary>
    /// A stupid agent that always just returns the cell in the direction of the target
    /// </summary>
    public class DummyNavigationAgent : IGridNavigationAgent
    {
        private readonly IGrid grid;
        private readonly IGridObject obj;

        public DummyNavigationAgent(IGrid grid, IGridObject obj)
        {
            this.grid = grid;
            this.obj = obj;
        }

        public Vector2I GetNextPosition(Vector2I target)
        {
            Vector2I from = grid.GetObjectPosition(obj);
            return grid.GetPositionInDirection(from, target - from);
        }

        public Vector2I[] GetPathTo(Vector2I target)
        {
            throw new System.NotImplementedException(); // Ya no it's a dummy
        }
    }
}