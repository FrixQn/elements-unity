using UnityEngine;

namespace Project.Core
{
    public struct SwipeData : ISwipeData
    {
        public Vector3 Delta { get; set; }

        public ISwappable Target { get; set; }

        public SwipeDirection Direction { get; set; }

        public Vector3 StartPosition { get; set; }

        public Vector3 EndPosition { get; set; }
    }

}
