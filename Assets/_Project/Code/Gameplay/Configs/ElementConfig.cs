using UnityEngine;

namespace Project.Gameplay
{
    [CreateAssetMenu(fileName = "ElementConfig", menuName = "Project/Configs/Gameplay/ElementConfig")]
    public class ElementConfig : ScriptableObject
    {
        [field: SerializeField] public Element Element { get; private set; }
    }
}
