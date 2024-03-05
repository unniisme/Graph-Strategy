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

        public string[] teams = {"blue", "red"};

        private int currentTeam = 0;
        public string PlayingTeam => teams[currentTeam];

        [Export]
        public BuildPhantom[] phantoms;

        public override void _Ready()
        {
            phantoms[currentTeam].TurnedOff += MadeMove;
            phantoms[currentTeam].On();
        }

        public void MadeMove()
        {
            phantoms[currentTeam].TurnedOff -= MadeMove;

            currentTeam = 1 - currentTeam;
            GD.Print($"Currently playing : {PlayingTeam}");

            phantoms[currentTeam].TurnedOff += MadeMove;
            phantoms[currentTeam].On();
        }


    }
}