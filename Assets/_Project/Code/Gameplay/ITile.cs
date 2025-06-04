using UnityEngine;

namespace Project.Gameplay
{
    public interface ITile
    {
        public IElement Element { get; }
        public Vector2Int Position { get; }
        public Vector3 WorldPosition { get; }
        public bool IsEmpty { get; }

        public void SetElement(IElement element);
    }
}
