using UnityEngine;

namespace Project.Gameplay
{
    [CreateAssetMenu(fileName = "GridLayoutConfig", menuName = "Project/Configs/Gameplay/GridLayoutConfig")]
    public class GridLayoutConfig : ScriptableObject
    {
        [field: SerializeField] public Vector2 ElementSize { get; set; } = new Vector2(1, 1);
        [field: SerializeField] public float GapX { get; set; } = 0.1f;
        [field: SerializeField] public float GapY { get; set; } = 0.1f;
        [field: SerializeField] public int LeftOffsetCells { get; set; } = 0;
        [field: SerializeField] public int RightOffsetCells { get; set; } = 0;
    }
}
