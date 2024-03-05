using Gamelogic.Grid;
using Gamelogic.Managers;
using Godot;

namespace Gamelogic.Objects
{
    [GlobalClass]
    public partial class Soldier : MovableGridObject, ISelectable
    {
        private Vector2I targetCell;
        private GodotGridNavigationAgent agent;

        [Export(PropertyHint.Enum, GameResources.TeamTypeHint)]
        public string teamName = "";
        public override string Tag => teamName;
        
        private static Soldier selectedSoldier = null;
        public static Soldier SelectedSoldier
        {
            get => selectedSoldier;
            set
            {
                selectedSoldier = value;
                GameManager.GetLevel().selectedSoldier = value;
            }
        }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                selectionSprite.Visible = value;
            }
        }
        private bool isSelected = false;
        private Sprite2D selectionSprite = null;

        public override void _Ready()
        {
            base._Ready();

            targetCell = GridPosition;
            agent = GetNode<GodotGridNavigationAgent>("GodotGridNavigationAgent");
            agent.SetGrid(GameManager.GetLevel().grid);
            GetNode<Area2D>("Area2D").InputEvent += OnInputEvent;

            selectionSprite = (Sprite2D)FindChild("Selection");
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (targetCell != GridPosition)
            {
                Vector2I nextCell = agent.GetNextPosition(targetCell);
                Move(nextCell - GridPosition);
            }

            if (Input.IsMouseButtonPressed(MouseButton.Left)
                && GameManager.GetLevel().CanAttack
                && isSelected)
            {
                Vector2I targetPos = grid.GameCoordinateToGridCoordinate(GetGlobalMousePosition());
                if (!agent.GetPathTo(targetPos).IsEmpty() && targetPos != GridPosition)
                {
                    targetCell = targetPos;
                    GameManager.GetLevel().MadeMove();
                }
            }
        }

        public void Select()
        {
            if (!IsSelected)
            {
                selectedSoldier?.Deselect();
                SelectedSoldier = this;
                IsSelected = true;
                GD.Print($"Selecting {Name}");
            }
        }
        public void Deselect()
        {
            if (IsSelected)
            {
                SelectedSoldier = null;
                IsSelected = false;
                GD.Print($"Deselecting {Name}");
            }
        }

        public void ToggleSelect()
        {
            if (IsSelected) Deselect();
            else Select();
        }

        public void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
        {
            if (@event is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsPressed() && GameManager.GetLevel().CanAttack)
                {
                    ToggleSelect();
                }
            }
        }
    }
}