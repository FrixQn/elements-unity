using UnityEngine;

namespace Project.Gameplay
{
    public struct Tile : ITile
    {
        public IElement Element { get; private set; }
        public Vector2Int Position { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        public readonly bool IsEmpty => Element == null;

        public Tile(Vector2Int position, Vector3 worldPosition, IElement element)
        {
            Position = position;
            WorldPosition = worldPosition;
            Element = element;
        }

        public void SetElement(IElement item) =>
            Element = item;
    }
}
