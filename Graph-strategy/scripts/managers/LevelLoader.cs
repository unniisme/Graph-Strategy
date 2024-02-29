using Godot;

namespace Gamelogic.Managers
{
    [GlobalClass]
    public partial class LevelLoader : Node2D
    {
        const string lev = "res://scenes/levels/test_scene.tscn";

        public override void _Ready()
        {
            base._Ready();

            GameManager.ChangeLevelTo(lev);
        }
    }
}