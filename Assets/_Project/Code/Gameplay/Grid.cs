using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Gameplay
{
    public struct Grid
    {
        public enum NeighbourDir { Top, Left, Right, Bottom }
        private Tile[,] _tiles;

        public Grid(int width, int height, Vector3[,] worldPositions, IElement[] elements)
        {
            _tiles = new Tile[width, height];
            int index;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < _tiles.GetLength(0) && y < _tiles.GetLength(1))
                    {
                        index = x * height + y;
                        if (index < width * height)
                        {
                            _tiles[x, y] = new Tile(new Vector2Int(x, y), worldPositions[x, y], elements[index]);
                        }
                    }
                }
            }
        }

        public readonly bool IsEmpty()
        {
            for (int y = 0; y < _tiles.GetLength(1); y++)
            {
                for (int x = 0; x < _tiles.GetLength(0); x++)
                {
                    if (!_tiles[x, y].IsEmpty)
                        return false;
                }
            }

            return true;
        }

        public readonly void SwapTiles(Vector2Int positionA, Vector2Int positionB)
        {
            if (!IsPositionValid(positionA) || !IsPositionValid(positionB))
                throw new ArgumentException("Invalid tile positions");

            ref Tile tileA = ref _tiles[positionA.x, positionA.y];
            ref Tile tileB = ref _tiles[positionB.x, positionB.y];

            IElement tileAElement = tileA.Element;
            IElement tileBElement = tileB.Element;

            tileA.SetElement(tileBElement);
            tileB.SetElement(tileAElement);
        }

        private readonly bool IsPositionValid(Vector2Int position)
        {
            return position.x >= 0 && position.x < _tiles.GetLength(0) && position.y >= 0 && position.y < _tiles.GetLength(1);
        }

        public readonly ITile[] GetTiles()
        {
            if (_tiles == null)
                return new ITile[0];

            int rows = _tiles.GetLength(0);
            int cols = _tiles.GetLength(1);
            ITile[] flat = new ITile[rows * cols];
            int index = 0;
            for (int y = 0; y < cols; y++)
                for (int x = 0; x < rows; x++)
                    flat[index++] = _tiles[x, y];
            return flat;
        }

        public readonly ITile GetElementTile(IElement element)
        {

            for (int y = 0; y < _tiles.GetLength(1); y++)
            {
                for (int x = 0; x < _tiles.GetLength(0); x++)
                {
                    if (_tiles[x, y].Element == element)
                        return _tiles[x, y];
                }
            }

            return null;
        }

        public readonly ITile GetNeighbourTile(ITile tile, NeighbourDir direction)
        {
            int x = tile.Position.x;
            int y = tile.Position.y;
            return direction switch
            {
                NeighbourDir.Left => (x > 0) ? _tiles[x - 1, y] : null,
                NeighbourDir.Right => (x < _tiles.GetLength(0) - 1) ? _tiles[x + 1, y] : null,
                NeighbourDir.Top => (y < _tiles.GetLength(1) - 1) ? _tiles[x, y + 1] : null,
                NeighbourDir.Bottom => (y > 0) ? _tiles[x, y - 1] : null,
                _ => null,
            };
        }

        public readonly void ClearTile(ITile tile)
        {
            _tiles[tile.Position.x, tile.Position.y].SetElement(null);
        }

        public readonly List<TileMoveInfo> NormalizeGrid()
        {
            List<TileMoveInfo> movedElements = new();

            int width = _tiles.GetLength(0);
            int height = _tiles.GetLength(1);
            Vector2Int positionFrom, positionTo;
            IElement elementToMove;

            for (int x = 0; x < width; x++)
            {
                int writeY = 0;

                for (int readY = 0; readY < height; readY++)
                {
                    if (!_tiles[x, readY].IsEmpty)
                    {
                        if (writeY != readY)
                        {
                            positionFrom = new(x, readY);
                            positionTo = new(x, writeY);
                            elementToMove = _tiles[positionFrom.x, positionFrom.y].Element;
                            SwapTiles(positionFrom, positionTo);

                            movedElements.Add(new TileMoveInfo(
                                elementToMove,
                                positionFrom,
                                positionTo,
                                _tiles[x, writeY].WorldPosition
                            ));
                        }
                        writeY++;
                    }
                }

                for (int y = writeY; y < height; y++)
                {
                    _tiles[x, y].SetElement(null);
                }
            }

            return movedElements;
        }

        public readonly List<List<ITile>> FindAllConnectedGroups(int minGroupSize = 3)
        {
            int width = _tiles.GetLength(0);
            int height = _tiles.GetLength(1);
            var visited = new HashSet<ITile>();
            var groups = new List<List<ITile>>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ITile tile = _tiles[x, y];
                    if (tile.IsEmpty || visited.Contains(tile))
                        continue;

                    var group = GetConnectedTiles(tile);
                    foreach (var t in group)
                        visited.Add(t);

                    if (group.Count >= minGroupSize && HasLineOfThreeOrMore(group))
                        groups.Add(group);
                }
            }

            return groups;
        }

        private readonly bool HasLineOfThreeOrMore(List<ITile> group)
        {
            var positions = group.Select(t => t.Position).ToList();

            if (HasConsecutiveLine(positions, pos => pos.y, pos => pos.x))
                return true;

            if (HasConsecutiveLine(positions, pos => pos.x, pos => pos.y))
                return true;

            return false;
        }

        private readonly bool HasConsecutiveLine(List<Vector2Int> positions,Func<Vector2Int, int> groupBySelector,
            Func<Vector2Int, int> orderBySelector)
        {
            foreach (var group in positions.GroupBy(groupBySelector))
            {
                var sorted = group.Select(orderBySelector).OrderBy(v => v).ToList();
                int count = 1;
                for (int i = 1; i < sorted.Count; i++)
                {
                    if (sorted[i] == sorted[i - 1] + 1)
                        count++;
                    else
                        count = 1;
                    if (count >= 3)
                        return true;
                }
            }
            return false;
        }

        private readonly List<ITile> GetConnectedTiles(ITile tile, List<ITile> exclude = null)
        {
            var result = new List<ITile>() { tile };
            if (tile.IsEmpty)
                return result;

            if (exclude == null)
                exclude = new List<ITile>() { tile };
            else
                exclude.Add(tile);

            ITile neighbour;
            foreach (NeighbourDir dir in Enum.GetValues(typeof(NeighbourDir)))
            {
                neighbour = GetNeighbourTile(tile, dir);
                if (neighbour == null || neighbour.IsEmpty || exclude.Contains(neighbour))
                    continue;

                if (neighbour.Element.Name != tile.Element.Name)
                    continue;

                result.AddRange(GetConnectedTiles(neighbour, exclude));
            }

            return result;
        }

    }
}
