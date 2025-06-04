using System;
using UnityEngine;

namespace Project.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "Project/Configs/Gameplay/GameplayConfig")]
    public class GameplayConfig : ScriptableObject
    {
        [field: SerializeField, Min(0.1f)] public float AnimationsDuration { get; private set; } = 0.3f;
        [field: SerializeField, Range(30, 120)] public int TargetFramerate { get; private set; } = 120;
        [SerializeField] private LevelConfig[] _levels;

        public LevelConfig GetLevelConfig(int levelNumber)
        {
            if (_levels == null || _levels.Length == 0)
                throw new InvalidOperationException("Levels count can't be less than zero");

            levelNumber = Mathf.Clamp(levelNumber, 0, int.MaxValue);
            int index = levelNumber % _levels.Length;
            return _levels[index];
        }
    }
}
