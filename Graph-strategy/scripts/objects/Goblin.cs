using Gamelogic.Grid;
using Gamelogic.Managers;
using Godot;

namespace Gamelogic.Objects
{
    [GlobalClass]
    public partial class Goblin : MovableGridObject
    {
        private Vector2I targetCell;
        private GodotGridNavigationAgent agent;
        public override string Tag => "red";

        public override void _Ready()
        {
            base._Ready();

            targetCell = GridPosition;
            agent = GetNode<GodotGridNavigationAgent>("GodotGridNavigationAgent");
            agent.SetGrid(GameManager.GetLevel().grid);

            GameManager.GetLevel().AttackEvent += NavigateToTarget;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (targetCell != GridPosition)
            {
                Vector2I nextCell = agent.GetNextPosition(targetCell);
                Move(nextCell - GridPosition);
            }
        }

        private void NavigateToTarget(Vector2I target)
        {
            targetCell = target;
        }
    }
}

