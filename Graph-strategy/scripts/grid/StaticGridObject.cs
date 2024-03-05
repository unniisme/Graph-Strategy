using Gamelogic.Grid;
using Gamelogic.Managers;
using Godot;

namespace Gamelogic.Objects
{
    public partial class StaticGridObject : Node2D, IGridObject
    {
        [Export]
        public GodotGrid grid = null;
		public virtual string Tag => "StaticGridObject";

		/// <summary>
		/// Whether this object snaps to position in the grid
		/// </summary>
		[Export]
		public bool snap = true;

		public virtual Vector2I GridPosition
		{
			get => grid.GetObjectPosition(this);
			set => grid.MoveObject(this, value);
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			grid ??= GameManager.GetLevel().grid;
			// Register on the grid
			grid.PlaceObject(this, grid.GameCoordinateToGridCoordinate(GlobalPosition));
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _PhysicsProcess(double delta)
		{
			// Snap to grid
			if (snap)
			{
				Position = grid.GridCoordinateToGameCoordinate(GridPosition);
			}
		}

        public virtual bool Kill(Node2D attacker)
        {
            return false; // Override or something
        }
    }
}

