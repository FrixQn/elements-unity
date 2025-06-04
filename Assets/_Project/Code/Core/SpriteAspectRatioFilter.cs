using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAspectRatioFilter : MonoBehaviour
{
    #region EDITOR
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SpriteAspectRatioFilter))]
    public class SpriteAspectRatioFitterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var instance = (SpriteAspectRatioFilter)target;

            instance._aspectMode = (AspectMode)UnityEditor.EditorGUILayout.EnumPopup("Aspect Mode", instance._aspectMode);

            if (instance._aspectMode != AspectMode.None)
            {
                UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_aspect)));
            }

            serializedObject.ApplyModifiedProperties();
            instance.OnValidate();
        }
    }
#endif
    #endregion

    public enum AspectMode
    {
        None,
        FitInParent,
        EnvelopeParent
    }

    [SerializeField] private AspectMode _aspectMode = AspectMode.None;
    [SerializeField, Min(0.001f)] private float _aspect = 1.0f;
    private Camera _camera;
    private SpriteRenderer _spriteRenderer;
    private Sprite _lastSprite;
    private Vector2 _lastScreenSize;

#if UNITY_EDITOR
    private void OnValidate() => SyncState(force: true);
#endif
    private void Awake() => SyncState(force: true);
    
    private void OnEnable() => SyncState(force: true);

    private void Update()
    {
        if (ScreenSizeChanged() || SpriteChanged())
            SyncState(force: true);
    }

    private bool ScreenSizeChanged() =>
        _lastScreenSize.x != Screen.width || _lastScreenSize.y != Screen.height;

    private bool SpriteChanged() =>
        _spriteRenderer != null && _spriteRenderer.sprite != _lastSprite;

    private void SyncState(bool force = false)
    {
        CacheComponentsIfNeeded();

        if (_spriteRenderer != null && _spriteRenderer.sprite != null && _lastSprite != _spriteRenderer.sprite)
        {
            var size = _spriteRenderer.sprite.bounds.size;
            if (size.y > 0f)
                _aspect = size.x / size.y;
            _lastSprite = _spriteRenderer.sprite;
        }

        ApplyAspect(force);
    }

    private void CacheComponentsIfNeeded()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_camera == null)
            _camera = Camera.main;
    }

    private void ApplyAspect(bool force = false)
    {
        if (_camera == null || _spriteRenderer == null || _spriteRenderer.sprite == null || _aspectMode == AspectMode.None)
            return;

        if (!force && !ScreenSizeChanged() && !SpriteChanged())
            return;

        float camHeight = _camera.orthographicSize * 2f;
        float camWidth = camHeight * _camera.aspect;
        Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;
        float targetWidth, targetHeight;

        switch (_aspectMode)
        {
            case AspectMode.FitInParent:
                targetWidth = camWidth;
                targetHeight = camWidth / _aspect;
                if (targetHeight > camHeight)
                {
                    targetHeight = camHeight;
                    targetWidth = camHeight * _aspect;
                }
                break;
            case AspectMode.EnvelopeParent:
                targetWidth = camWidth;
                targetHeight = camWidth / _aspect;
                if (targetHeight < camHeight)
                {
                    targetHeight = camHeight;
                    targetWidth = camHeight * _aspect;
                }
                break;
            default:
                return;
        }

        float scaleX = targetWidth / spriteSize.x;
        float scaleY = targetHeight / spriteSize.y;
        transform.localScale = new Vector3(scaleX, scaleY, 1f);

        _lastScreenSize = new Vector2(Screen.width, Screen.height);
    }
}