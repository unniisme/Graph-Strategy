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

        public Soldier selectedSoldier = null;

        public void MadeMove()
        {
            currentTeam = 1 - currentTeam;
            GD.Print($"Currently playing : {PlayingTeam}");
        }
    }
}