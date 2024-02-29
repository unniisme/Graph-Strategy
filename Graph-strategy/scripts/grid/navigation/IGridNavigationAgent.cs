using Godot;

namespace Gamelogic.Grid
{
    /// <summary>
    /// Interface for different agents that can be used to navigate the grid 
    /// </summary>
    public interface IGridNavigationAgent
    {
        // Should have a constructor with roughly the following signature
        /// 
        /// public GridNavigationAgent(IGrid grid, Node2D obj)
        /// 
        /// This will save the grid and the agent obj in this instance
        /// 
        /// For the pathfinding functions in this agent, the start cell will always be
        /// grid.GetObjectPosition(this.obj)
        /// 
        /// It could also have other parameters like maximum distance, heuristics etc

        /// <summary>
        /// Function to find the next step in navigation
        /// </summary>
        /// <param name="target">Target to navigate this agent to</param>
        /// <returns>The next cell index to move to to navigate to the target</returns>
        public Vector2I GetNextPosition(Vector2I target);

        /// <summary>
        /// Get the entire sequence of cells requred to reach a target
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Vector2I[] GetPathTo(Vector2I target);
    }
}