using Project.Core;
using System;
using UnityEngine;

namespace Project.Core
{
    [CreateAssetMenu(fileName = "BallonSpawnerConfig", menuName = "Project/Configs/Core/BallonSpawnerConfig")]
    public class BalloonControllerConfig : ScriptableObject
    {
        [field: SerializeField] public Balloon[] Prefabs { get; private set; } = Array.Empty<Balloon>();
        [field: SerializeField] public float Distance { get; private set; } = 10f;
        [field: SerializeField] public float MinSpeed { get; private set; } = 1f;
        [field: SerializeField] public float MaxSpeed { get; private set; } = 4f;
        [field: SerializeField, Min(0.1f)] public float SpawnInterval { get; private set; } = 1f;
        [field: SerializeField] public float MinCurveHeight { get; private set; } = 0.5f;
        [field: SerializeField] public float MaxCurveHeight { get; private set; } = 3f;
        [field: SerializeField] public float MinFrequency { get; private set; } = 0.5f;
        [field: SerializeField] public float MaxFrequency { get; private set; } = 2f;
        [field: SerializeField] public float SpawnOffset { get; private set; } = 1f;
        [field: SerializeField, Min(1)] public int BallonsLimit { get; private set; } = 3;
    }
}
