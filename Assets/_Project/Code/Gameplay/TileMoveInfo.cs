using UnityEngine;

namespace Project.Gameplay
{
    public class TileMoveInfo
    {
        public IElement Element { get; }
        public Vector2Int From { get; }
        public Vector2Int To { get; }
        public Vector3 ToWorldPosition { get; }

        public TileMoveInfo(IElement element, Vector2Int from, Vector2Int to, Vector3 toWorldPosition)
        {
            Element = element;
            From = from;
            To = to;
            ToWorldPosition = toWorldPosition;
        }
    }

}
