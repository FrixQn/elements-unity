using System;

namespace Project.Gameplay
{
    [Serializable]
    public struct LevelState
    {
        public int Level;
        public TileInfo[] Tiles;
    }
}
