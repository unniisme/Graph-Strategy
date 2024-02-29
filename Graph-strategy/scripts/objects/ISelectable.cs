namespace Gamelogic.Objects
{
    public interface ISelectable
    {
        public void Select();
        public void Deselect();
        public void ToggleSelect();
    }
}