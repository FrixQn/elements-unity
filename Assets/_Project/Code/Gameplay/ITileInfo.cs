using UnityEngine;

namespace Project.Gameplay
{
    public interface ITileInfo
    {
        public Vector2Int Position { get; set; }
        public string Element { get; set; }
    }
}
