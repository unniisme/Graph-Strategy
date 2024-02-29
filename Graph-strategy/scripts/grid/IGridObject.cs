using Godot;

namespace Gamelogic.Grid
{
    /// <summary>
    /// An object placed on a grid
    /// </summary>
    public interface IGridObject
    {
        /// <summary>
        /// Position of this object on the grid
        /// </summary>
        public Vector2I GridPosition {get;}

        /// <summary>
        /// Try to move the object in "dir" direction
        /// </summary>
        /// <param name="to"></param>
        /// <returns>Whether the movement was possible</returns>
        public bool Move(Vector2 dir);

        /// <summary>
        /// Function to try and destroy the object, or atleast move it out of its current position
        /// </summary>
        /// <param name="to">The entity causing the distruction</param>
        /// <returns>Whether the action was successfull</returns>
        public bool Kill(Node2D attacker);

        public StringName Name {get;}
        public string Tag {get;}
    }
}