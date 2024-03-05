using Gamelogic.Managers;
using Gamelogic.Objects;
using Godot;

namespace Gamelogic.Grid
{
	/// <summary>
	/// Tilemap that registers one of its layers to the grid
	/// </summary>
	[GlobalClass]
	public partial class GridTileMap : TileMap
	{
		[Export]
		public GodotGrid grid = null;

		/// <summary>
		/// Index of layer to register to grid
		/// </summary>
		[Export]
		public int gridLayer;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			grid ??= GameManager.GetLevel().grid;

			foreach (Vector2I pos in GetUsedCells(gridLayer))
			{
                StaticGridObject obj = new()
                {
                    grid = grid
                };
                grid.PlaceObject(obj, pos);
			} 
		}
	}
}
