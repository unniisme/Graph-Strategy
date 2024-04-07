using System.Collections.Generic;
using Gamelogic.Managers;

namespace Graphs
{
    /// <summary>
    /// Class represents either an island - a well connected collection of nodes
    ///     or a bridge - a chain of degree 2 edges
    /// </summary>
    public class Islet<T> : HashSet<T>
    {
        public bool IsIsland; // Bridge if false
        public int UID = GameResources.UIDgen;

        
        public Islet(bool isIsland)
        {
            IsIsland = isIsland;
        }
    }
}