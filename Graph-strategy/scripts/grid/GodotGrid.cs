using System;
using System.Collections.Generic;
using Godot;

namespace Gamelogic.Grid
{
    [GlobalClass]
    public partial class GodotGrid : Node2D, IGrid
    {
        private readonly Dictionary<IGridObject, Vector2I> objectToIndex = new();
        private readonly Dictionary<Vector2I, IGridObject> indexToObject = new();
        private readonly List<IGridObject> objects = new();

        [Export]
        public Vector2 Offset { get; set; }
        public event Action<Vector2I> GridChangeEvent;
        public List<IGridObject> PlacedObjects => objects;

        public GodotGrid(Vector2 scale, Vector2 offset)
        {
            Scale = scale;
            Offset = offset;
        }

        public GodotGrid(Vector2 scale)
        {
            Scale = scale;
            Offset = Vector2.Zero;
        }

        public GodotGrid()
        {
        }

        public Vector2I GameCoordinateToGridCoordinate(Vector2 pos)
        {
            return (Vector2I)((pos - Offset)/Scale).Round();
        }
        public Vector2 GridCoordinateToGameCoordinate(Vector2I pos)
        {
            return ((Vector2)pos * Scale) + Offset;
        }

        public IGridObject GetObject(Vector2I pos)
        {
            if (indexToObject.ContainsKey(pos))
            {
                return indexToObject[pos];
            }
            return null;
        }

        public Vector2I GetObjectPosition(IGridObject obj)
        {
            if (objectToIndex.ContainsKey(obj))
            {
                return objectToIndex[obj];
            }
            throw new GridException("Object not present in grid", obj);
        }

        public Vector2 GetObjectPositionInGameCoordinates(IGridObject obj)
        {
            return GridCoordinateToGameCoordinate(GetObjectPosition(obj));
        }

        public void PlaceObject(IGridObject obj, Vector2I pos)
        {
            if (objectToIndex.ContainsKey(obj))
                throw new GridException("Object is already present in grid", pos, obj);
            if (indexToObject.ContainsKey(pos))
                throw new GridException("Position is already occupied", pos, obj);

            objectToIndex[obj] = pos;
            indexToObject[pos] = obj;
            objects.Add(obj);
            GridChangeEvent?.Invoke(pos);
        }

        public void PlaceObject(Node2D obj)
        {
            // Could throw an error
            PlaceObject((IGridObject)obj, GameCoordinateToGridCoordinate(obj.GlobalPosition));
        }

        public bool MoveObject(IGridObject obj, Vector2I pos)
        {
            if (indexToObject.ContainsKey(pos))
            {
                return false;
            }
            try
            {
                RemoveObject(obj);
                PlaceObject(obj, pos);
                return true;
            }
            catch (GridException)
            {
                return false;
            }
        }

        public bool MoveObject(Vector2I from, Vector2I to)
        {
            IGridObject obj = GetObject(from);
            return MoveObject(obj, to);
        }

        public Vector2I GetPositionInDirection(Vector2I pos, Vector2 dir)
        {
            Vector2I targetPos = (Vector2I)dir.Normalized().Round();
            return pos + targetPos;
        }


        public bool MoveObjectInDirection(IGridObject obj, Vector2 dir)
        {
            return MoveObject(obj, GetPositionInDirection(GetObjectPosition(obj), dir));
        }

        public bool MoveObjectInDirection(Vector2I pos, Vector2 dir)
        {
            return MoveObject(pos, GetPositionInDirection(pos, dir));
        }


        public void RemoveObject(Vector2I pos)
        {
            if (!indexToObject.ContainsKey(pos))
            {
                throw new GridException("Position not occupied", pos);
            }
            IGridObject obj = indexToObject[pos];
            RemoveObjectInternal(obj, pos);
        }

        public void RemoveObject(IGridObject obj)
        {
            if (!objectToIndex.ContainsKey(obj))
            {
                throw new GridException("Object not present in grid", obj);
            }
            Vector2I pos = objectToIndex[obj];
            RemoveObjectInternal(obj, pos);
        }

        private void RemoveObjectInternal(IGridObject obj, Vector2I pos)
        {
            // Do not use carelessly
            objectToIndex.Remove(obj);
            indexToObject.Remove(pos);
            objects.Remove(obj);
            GridChangeEvent?.Invoke(pos);
        }

        public IGridObject GridCast(Vector2I from, Vector2I to, int distance)
        {
            foreach (Vector2I target in BresenhamLine(from, to))
            {
                if (target == from) continue;
                
                distance--;
                if (distance == 0) return null;
                
                IGridObject obj = GetObject(target);
                if (obj != null) return obj;
            }
            return null;
        }


        public static IEnumerable<Vector2I> BresenhamLine(Vector2I from, Vector2I to)
        {
            int dx = Math.Abs(to.X - from.X);
            int dy = Math.Abs(to.Y - from.Y);
            int sx = (from.X < to.X) ? 1 : -1;
            int sy = (from.Y < to.Y) ? 1 : -1;

            int err = dx - dy;

            while (true)
            {
                yield return new Vector2I(from.X, from.Y);

                if (from.X == to.X && from.Y == to.Y)
                    break;

                int err2 = 2 * err;

                if (err2 > -dy)
                {
                    err -= dy;
                    from.X += sx;
                }

                if (err2 < dx)
                {
                    err += dx;
                    from.Y += sy;
                }
            }
        }
    }
}