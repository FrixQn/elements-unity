using UnityEngine;

namespace Project.Gameplay
{
    public struct TileInfo : ITileInfo
    {
        public Vector2Int Position { get; set; }
        public string Element { get; set; }
    }
}
