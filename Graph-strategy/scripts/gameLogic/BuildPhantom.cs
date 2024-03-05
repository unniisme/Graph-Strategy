using Gamelogic.Grid;
using Gamelogic.Managers;
using Gamelogic.Objects;
using Godot;
using System;

namespace Gamelogic
{
    [GlobalClass]
    public partial class BuildPhantom : Sprite2D, IGridObject
    {
        [Export]
        public GodotGrid grid;

        [Export]
        public string teamName = "";
        public string Tag => teamName;

        [Export(PropertyHint.File, "*.tscn")]
        public string towerScenePath;

        [Export]
        public int distanceToOtherTowers = 2;

        public event Action TurnedOff;

        public Vector2I GridPosition => grid.GameCoordinateToGridCoordinate(Position);

        public override void _Ready()
        {
            Off();
        }

        public override void _Process(double delta)
        {
            if (Visible)
            {
                Position = grid.GridCoordinateToGameCoordinate(
                    grid.GameCoordinateToGridCoordinate(GetGlobalMousePosition())
                );
                if (Input.IsActionJustReleased("Click"))
                {   
                    if (CanPlace())
                    {
                        PackedScene towerScene = ResourceLoader.Load<PackedScene>(towerScenePath);
                        Tower towerNode = towerScene.Instantiate<Tower>();
                        towerNode.Position = Position;
                        towerNode.grid = grid;
                        GameManager.GetLevel().AddChild(towerNode);
                        Off();
                    }
                }
            }

        }

        private bool CanPlace()
        {
            foreach (IGridObject obj in grid.PlacedObjects)
            {
                Vector2I gridPosition = GridPosition;
                if (GridPosition == obj.GridPosition)
                {
                    return false;
                }

                Vector2I diff = obj.GridPosition - gridPosition;
                if (MathF.Abs(diff.X + diff.Y) <= distanceToOtherTowers && obj.Tag != Tag)
                {
                    GD.Print($"Too close to {obj.Name} at {obj.GridPosition}");
                    return false;
                }
            }
            return true;
        }


        public void On()
        {
            Visible = true;
        }

        public void Off()
        {
            Visible = false;
            TurnedOff?.Invoke();
        }
    }
}
