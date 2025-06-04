using UnityEngine;

namespace Project.Core
{
    public class Balloon : MonoBehaviour
    {
        private IMovementFunction _movementFunction;
        public BalloonController.MoveDirection MoveDirection { get; private set; }
        private float _time;
        private float _speed;

        public void Initialize(IMovementFunction movementFunction, float speed, BalloonController.MoveDirection moveDirection)
        {
            _movementFunction = movementFunction;
            _speed = speed;
            MoveDirection = moveDirection;
        }

        public void Tick()
        {
            _time += _speed * Time.deltaTime;
            transform.position = _movementFunction.Evaluate(_time);
        }

        public void Destroy() =>
            Destroy(gameObject);
    }
}
