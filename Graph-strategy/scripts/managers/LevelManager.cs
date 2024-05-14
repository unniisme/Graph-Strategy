using System;
using Gamelogic.Grid;
using Gamelogic.Objects;
using Godot;

namespace Gamelogic.Managers
{
    [GlobalClass]
    public partial class LevelManager : Node2D
    {
        [Export]
        public GodotGrid grid;

        [Export]
        public GodotGrid buildGrid;

        public string[] teams = {"blue", "red"};

        [Export]
        public int currentTeam {get; private set;} = 0;
        public string PlayingTeam => teams[currentTeam];

        public Soldier selectedSoldier = null;

        [Export]
        public BuildPhantom[] phantoms; // Blue followed by red

        [Export]
        public int totalMoves = 5;

        private int movesElapsed = 0;
        public bool CanAttack => movesElapsed >= totalMoves*2;

        [Export]
        public Node2D humanTarget;

        public event Action<Vector2I> AttackEvent;

        public override void _Ready()
        {
            phantoms[currentTeam].TurnedOff += MadeMove;
            phantoms[currentTeam].On();
        }

        public void MadeMove()
        {
            movesElapsed += 1;
            if (!CanAttack)
            {
                phantoms[currentTeam].TurnedOff -= MadeMove;

                currentTeam = 1 - currentTeam;
                GD.Print($"Currently playing : {PlayingTeam}");

                phantoms[currentTeam].TurnedOff += MadeMove;
                phantoms[currentTeam].On();
            }
            else
            {
                foreach (IGridObject obj in buildGrid.PlacedObjects)
                {
                    if (obj is Tower tower)
                    {
                        if (tower.Tag == GameResources.BlueTeamName)
                        {
                            if (grid.GetObject(tower.GridPosition) == null)
                                grid.PlaceObject(tower);
                        }
                    }
                }

                AttackEvent?.Invoke(grid.GameCoordinateToGridCoordinate(humanTarget.Position));
            }
        }


    }
}