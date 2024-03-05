using Godot;

namespace Gamelogic.Managers
{
    public partial class GameResources : Node
    {
        public const string RedTeamName = "red";
        public const string BlueTeamName = "blue";
        public const string TeamTypeHint = "blue,red";

        public static string OtherTeam(string teamName) 
            => teamName switch
            {
                RedTeamName => BlueTeamName,
                BlueTeamName => RedTeamName,
                _ => ""
            };
    }
}