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

        [Export(PropertyHint.Enum, GameResources.TeamTypeHint)]
        public string teamName = "";
        public string Tag => teamName;

        [Export(PropertyHint.File, "*.tscn")]
        public string towerScenePath;

        [Export]
        public int distanceToOtherTowers = 2;

        public event Action TurnedOff;

        public Vector2I GridPosition => grid.GameCoordinateToGridCoordinate(Position);

        private float alpha = 0.5f;

        public override void _Ready()
        {
            Off();
            alpha = Modulate.A;
        }

        public override void _Process(double delta)
        {
            if (Visible)
            {
                Position = grid.GridCoordinateToGameCoordinate(
                    grid.GameCoordinateToGridCoordinate(GetGlobalMousePosition())
                );
                if (CanPlace())
                {
                    Color mod = Modulate;
                    mod.A = alpha;
                    Modulate = mod;
                }
                else
                {
                    Color mod = Modulate;
                    mod.A = alpha/4;
                    Modulate = mod;
                }

                if (Input.IsActionJustReleased("Click"))
                {   
                    if (CanPlace(true))
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

        private bool CanPlace(bool debug = false)
        {
            foreach (IGridObject obj in grid.PlacedObjects)
            {
                Vector2I gridPosition = GridPosition;
                if (GridPosition == obj.GridPosition)
                {
                    return false;
                }

                Vector2I diff = (obj.GridPosition - gridPosition).Abs();
                if ((diff.X + diff.Y) <= distanceToOtherTowers && obj.Tag == GameResources.OtherTeam(Tag))
                {
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
