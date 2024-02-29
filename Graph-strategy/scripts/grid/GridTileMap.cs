using Gamelogic.Managers;
using Godot;

namespace Gamelogic.Grid
{
	/// <summary>
	/// Tilemap that registers one of its layers to the grid
	/// </summary>
	[GlobalClass]
	public partial class GridTileMap : TileMap
	{
		public IGrid grid = null;

		/// <summary>
		/// Index of layer to register to grid
		/// </summary>
		[Export]
		public int gridLayer;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			grid ??= GameManager.GetLevel().grid;

			TileSet.TileSize = (Vector2I)grid.Scale;

			foreach (Vector2I pos in GetUsedCells(gridLayer))
			{
				grid.PlaceObject(new MovableGridObject(), pos);
			} 
		}
	}
}
