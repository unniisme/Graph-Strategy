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

        public StringName Name {get;}
        public string Tag {get;}
    }
}