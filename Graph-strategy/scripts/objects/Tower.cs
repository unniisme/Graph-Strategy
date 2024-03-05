using Gamelogic.Managers;
using Godot;

namespace Gamelogic.Objects
{
    [GlobalClass]
    public partial class Tower : StaticGridObject
    {
        [Export(PropertyHint.Enum, GameResources.TeamTypeHint)]
        public string teamName = "";
        public override string Tag => teamName;
    }
}