using DG.Tweening;
using Project.Core;
using UnityEngine;

namespace Project.Gameplay
{
    public interface IElement : ISwappable
    {
        public string Name { get; }
        public Tween Move(Vector3 position, float duration);
        public void SetSiblingIndex(int index);

        public void Destroy(float delay = 0f);
    }

}
