using UnityEngine;

namespace Project.Core
{
    public interface ISwipeData
    {
        public SwipeDirection Direction { get; }
        public Vector3 StartPosition { get; }
        public Vector3 EndPosition { get; }
        public Vector3 Delta { get; }
        public ISwappable Target { get; }
    }

}
