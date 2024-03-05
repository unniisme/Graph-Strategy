using Godot;

namespace Gamelogic.Objects
{
    [GlobalClass]
    public partial class Tower : StaticGridObject
    {
        [Export]
        public string teamName = "";
        public override string Tag => teamName;
    }
}