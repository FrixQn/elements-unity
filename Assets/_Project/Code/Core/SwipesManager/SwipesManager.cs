using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Core
{
    public class SwipesManager : MonoBehaviour, ISwipesManager
    {
        public enum RaycastMode { Physics3D, Physics2D }

        private readonly RaycastHit2D[] _hits2d = new RaycastHit2D[1];
        private readonly RaycastHit[] _hits3d = new RaycastHit[1];

        [SerializeField] private LayerMask _detectingLayers = -1;
        [SerializeField, Min(0f)] private float minSwipeDistance = 0f;
        [SerializeField] private RaycastMode raycastMode = RaycastMode.Physics3D;
        [SerializeField] private InputActionProperty _pointerInput;
        [SerializeField] private InputActionProperty _pointerPositionInput;
        private Camera _camera;
        private Vector2 _pointerStartPosition;
        private ISwappable _target;
        private SwipeData _data = default;
        private bool isSwiping = false;

        public event OnSwipeDetectedDelegate SwipeDetected;

        private void Awake()
        {
            SetupMainCameraIfPossible();
        }

        private void OnEnable()
        {
            _pointerInput.action.Enable();
            _pointerPositionInput.action.Enable();

            _pointerInput.action.performed += OnPointerPerformed;
        }

        private void OnDisable()
        {
            _pointerInput.action.performed -= OnPointerPerformed;

            _pointerInput.action.Disable();
            _pointerPositionInput.action.Disable();
        }

        private void OnPointerPerformed(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValueAsButton())
            {
                isSwiping = true;
                _pointerStartPosition = _pointerPositionInput.action.ReadValue<Vector2>();
                _target = GetSwappableUnderPointer(_pointerStartPosition);
            }
            else
            {
                if (!isSwiping) return;

                Vector2 pointerEndPosition = _pointerPositionInput.action.ReadValue<Vector2>();
                float distance = Vector2.Distance(_pointerStartPosition, pointerEndPosition);

                if (distance >= minSwipeDistance && _target != null)
                {
                    Vector2 direction = (pointerEndPosition - _pointerStartPosition).normalized;
                    UpdateSwipeData(GetSwipeDirection(direction), _camera.ScreenToWorldPoint(_pointerStartPosition), 
                        _camera.ScreenToWorldPoint(pointerEndPosition), _target);
                    OnSwipe(_target);
                }

                isSwiping = false;
                _target = null;
            }
        }

        private ISwappable GetSwappableUnderPointer(Vector2 screenPosition)
        {
            SetupMainCameraIfPossible();
            if (_camera == null)
                return null;

            Ray ray = _camera.ScreenPointToRay(screenPosition);

            switch (raycastMode)
            {
                case RaycastMode.Physics2D:
                    if (Physics2D.GetRayIntersectionNonAlloc(ray, _hits2d, float.PositiveInfinity, _detectingLayers) > 0)
                    {
                        if (_hits2d[0].collider.TryGetComponent(out ISwappable swappable))
                            return swappable;
                    }
                    break;
                case RaycastMode.Physics3D:
                default:
                    if (Physics.RaycastNonAlloc(ray, _hits3d, float.PositiveInfinity, _detectingLayers) > 0)
                    {
                        if (_hits3d[0].collider.TryGetComponent(out ISwappable swappable))
                            return swappable;
                    }
                    break;
            }

            return null;
        }

        private void SetupMainCameraIfPossible()
        {
            _camera = _camera != null ? _camera : Camera.main;
        }

        private void UpdateSwipeData(SwipeDirection swipeDirection, Vector3 startPosition, Vector3 endPosition, ISwappable target)
        {
            _data.Direction = swipeDirection;
            _data.StartPosition = startPosition;
            _data.EndPosition = endPosition;
            _data.Delta = endPosition - startPosition;
            _data.Target = target;
        }

        private void OnSwipe(ISwappable target)
        {
            SwipeDetected?.Invoke(_data);
            TriggerTarget(target);
        }

        private void TriggerTarget(ISwappable target)
        {
            if (target == null)
                return;

            target.OnSwipe(_data);
        }

        private SwipeDirection GetSwipeDirection(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return direction.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            else
                return direction.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }
    }

    public enum SwipeDirection
    {
        Up,
        Down,
        Left,
        Right
    }
}
