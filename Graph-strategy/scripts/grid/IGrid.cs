using System;
using System.Collections.Generic;
using Godot;

namespace Gamelogic.Grid
{
    /// <summary>
    /// Represents a 2D grid where objects can be kept, moved etc
    /// </summary>
    public interface IGrid
    {
        /// <summary>
        /// Event that is triggered when any change occurs in the grid
        /// </summary>
        public event Action<Vector2I> GridChangeEvent; 

        /// <summary>
        /// Scale in x and y direction
        /// Scale.x is how much distance in the x direction of game coordinates in 1 cell of the grid
        /// </summary>
        public Vector2 Scale {get; set;}

        /// <summary>
        /// Offset from the center of the game coordinate system where the (0,0) cell of the grid starts
        /// </summary>
        public Vector2 Offset {get; set;}

        /// <summary>
        /// Given a position in game coordinates, returns the corresponding cell in the grid
        /// </summary>
        /// <param name="pos">Game coordinates</param>
        /// <returns>Grid coordinates</returns>
        public Vector2I GameCoordinateToGridCoordinate(Vector2 pos);

        /// <summary>
        /// Given the index of a grid cell, return the center of that cell in game coordinates
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector2 GridCoordinateToGameCoordinate(Vector2I pos);

        /// <summary>
        /// List of objects placed on the grid
        /// </summary>
        public List<IGridObject> PlacedObjects {get;}

        /// <summary>
        /// Place a node in the given cell in the grid
        /// </summary>
        /// <param name="obj">The object being placed in the grid./param>
        /// <param name="pos">The index of the cell to place this object</param>
        /// <exception cref="GridException">
        /// When the object is unable to be placed
        /// </exception>
        public void PlaceObject(IGridObject obj, Vector2I pos);

        /// <summary>
        /// Place object contextually
        /// </summary>
        /// <param name="obj">Must be a <see cref="IGridObject"/><</param>
        public void PlaceObject(Node2D obj);

        /// <summary>
        /// Get the object in the given cell position
        /// returns null if cell doesn't have an object
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public IGridObject GetObject(Vector2I pos);

        /// <summary>
        /// Get the position of a given object
        /// </summary>
        /// <param name="obj">Object on grid</param>
        /// <returns>Cell index of the object</returns>
        public Vector2I GetObjectPosition(IGridObject obj);

        /// <summary>
        /// Get position of a given object in game coordinates
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Vector2 GetObjectPositionInGameCoordinates(IGridObject obj);

        /// <summary>
        /// Remove the object in the given cell
        /// Returns false if there is no object in the given cell
        /// </summary>
        /// <param name="pos"></param>
        /// <exception cref="GridException">
        /// When the object is unable to be placed
        /// </exception>
        public void RemoveObject(Vector2I pos);

        /// <summary>
        /// Remove the given object from the grid
        /// Returns false if the object is not present in the grid
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="GridException">
        /// When the object is unable to be placed
        /// </exception>
        public void RemoveObject(IGridObject obj);

        /// <summary>
        /// Move the given object to the given position
        /// 
        /// Returns false if there is already an object in the given position,
        /// or if the object is not on the grid
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pos"></param>
        /// <returns>Whether the movement was successfull</returns>
        public bool MoveObject(IGridObject obj, Vector2I pos);

        /// <summary>
        /// Move the object in "from" cell to "to" cell
        /// 
        /// Returns false if the to cell already has an object,
        /// or if the from cell does not have an object
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>Whether the movement was successfull</returns>

        public bool MoveObject(Vector2I from, Vector2I to);

        /// <summary>
        /// Move the given object in the given direction
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="dir"></param>
        /// <returns>Whether the movement was successfull</returns>
        public bool MoveObjectInDirection(IGridObject obj, Vector2 dir);

        /// <summary>
        /// Move object in cell "pos" on the grid in the given direction
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        /// <returns>Whether the movement was successfull</returns>
        public bool MoveObjectInDirection(Vector2I pos, Vector2 dir);

        /// <summary>
        /// Get the adjacent cell in the given direction
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Vector2I GetPositionInDirection(Vector2I pos, Vector2 dir);

        /// <summary>
        /// Cast a ray through the grid and see if it hits something
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns>The hit object. Null if none</returns>
        public IGridObject GridCast(Vector2I from, Vector2I to, int distance);
    }
}