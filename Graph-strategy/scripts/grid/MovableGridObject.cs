using Gamelogic.Managers;
using Godot;

namespace Gamelogic.Grid
{
	[GlobalClass]
	/// <summary>
	/// A godot object that is suppose to reside in a grid
	/// </summary>
	public partial class MovableGridObject : Node2D, IGridObject
	{

		// Lerping
		internal bool isMoving = false;
		[Export]
		internal float moveTime = 0.1f; // Time to animate movement in seconds
		internal float movementFraction = 0f; 
		internal Vector2 initialVector;
		internal Vector2 finalVector;
		internal bool alive = true;
		private bool movable = false;

		public IGrid grid = null;
		public virtual string Tag => "GridObject";

		/// <summary>
		/// Whether this object snaps to position in the grid
		/// </summary>
		[Export]
		public bool snap = true;

		/// <summary>
		/// Whether to update sprite y coordinates using z coordinates on the grid
		/// </summary>
		[Export]
		public bool setLayerZ = true;

		public virtual bool Movable => movable;
		[Export]
		public virtual Vector2I GridPosition
		{
			get => grid.GetObjectPosition(this);
			set => grid.MoveObject(this, value);
		}

		public MovableGridObject() {}
		

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
			if (!alive) return;

			// Snap to grid
			if (snap && !isMoving)
			{
				Position = grid.GridCoordinateToGameCoordinate(GridPosition);
			}
			else if (isMoving)
			{
				ProcessMove((float)delta);
			}

			if (setLayerZ)
			{
				ZIndex = GridPosition.Y;
			}
		}

		public bool Move(Vector2 dir)
		{
			if (isMoving) return false;
			if (grid.MoveObjectInDirection(this, dir))
			{
				SetMoving();
				return true;
			}
			return false;
		}

		internal void SetMoving()
		{
			isMoving = true;
			initialVector = Position;
			finalVector = grid.GridCoordinateToGameCoordinate(GridPosition);
			movementFraction = 0;
		}
		internal void UnsetMoving()
		{
			isMoving = false;
		}
		internal virtual void ProcessMove(float delta)
		{
			Position = initialVector.Lerp(finalVector, movementFraction);
			movementFraction += delta/moveTime;
			if (movementFraction >= 1)
				UnsetMoving();
		}

		public virtual bool Kill(Node2D attacker)
		{
			return false; // Override
		}
	}
}
