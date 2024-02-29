using Godot;

namespace Gamelogic.Managers
{
    public partial class GameManager : Node
    {
        private static GameManager runningGame = null;
        private static LevelManager runningLevel;
        public static void RegisterLevel(LevelManager level) => runningLevel = level;
        public static LevelManager GetLevel() => runningLevel;

        public override void _Ready()
        {
            runningGame = this;

            FindLevel();
        }

        public static void ChangeLevelTo(StringName path)
        {
            runningGame.GetTree().ChangeSceneToFile(path);

            FindLevel();
        }

        private static LevelManager FindLevel()
        {
            foreach (Node child in runningGame.GetTree().Root.GetChildren())
            {
                if (child is LevelManager level)
                {
                    RegisterLevel(level);
                    return level;                    
                }
            }
            return null;
        }
    }
}