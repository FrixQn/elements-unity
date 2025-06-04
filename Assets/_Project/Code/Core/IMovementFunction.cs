using UnityEngine;

namespace Project.Core
{
    public interface IMovementFunction
    {
        public Vector3 Evaluate(float time);
    }
}
