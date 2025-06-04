using System;
using UnityEngine;
using static Project.Gameplay.LevelConfig;

namespace Project.Gameplay
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Project/Configs/Gameplay/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Serializable]
        public class TileConfig
        {
#if UNITY_EDITOR
            [SerializeField, HideInInspector] public string EditorName;
#endif
            [SerializeField] private ElementConfig _element;
            public Element Element => _element?.Element;
            public ElementConfig ElementConfig => _element;
        }

        [field: SerializeField] public int Width { get; private set; }
        [field: SerializeField] public int Height { get; private set; }
        [SerializeField] private TileConfig[] _tiles;

        #region OnValidate
#if UNITY_EDITOR
        private void OnValidate()
        {
            int arrayLength = _tiles.Length;
            int gridLength = Width * Height;
            if (arrayLength < gridLength || arrayLength > gridLength)
            {
                Array.Resize(ref _tiles, Width * Height);
            }

            TileConfig tileConfig;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    tileConfig = _tiles[y * Width + x];
                    if (tileConfig != null)
                        tileConfig.EditorName = $"({y},{x}) {(tileConfig.Element == null ? "Empty" : $"{tileConfig.Element.name}")}";
                }
            }
        }
#endif
        #endregion

        public ElementConfig GetElementConfig(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) 
                return null;

            return _tiles[y * Width + x].ElementConfig;
        }

        public ElementConfig GetElementConfigByElementName(string name)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (name == _tiles[y * Width + x].ElementConfig?.name)
                        return _tiles[y * Width + x].ElementConfig;
                }
            }

            return null;
        }
    }
}
