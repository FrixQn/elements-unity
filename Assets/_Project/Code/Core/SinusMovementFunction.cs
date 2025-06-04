using UnityEngine;

namespace Project.Core
{
    public class SinusMovementFunction : IMovementFunction
    {
        private readonly float _distance;
        private readonly Vector3 _startPosition;
        private readonly int _direction;
        private readonly float _amplitude;
        private readonly float _frequency;

        public SinusMovementFunction(float distance, Vector3 startPosition, int direction, float amplitude, float frequency)
        {
            _distance = distance;
            _startPosition = startPosition;
            _direction = direction;
            _amplitude = amplitude;
            _frequency = frequency;
        }

        public Vector3 Evaluate(float time)
        {
            float progress = Mathf.Clamp01(time);

            float x = _direction * Mathf.Lerp(0, _distance, progress);
            float y = Mathf.Sin(progress * Mathf.PI * 2 * _frequency) * _amplitude;

            return _startPosition + new Vector3(x, y, 0);
        }
    }

}
