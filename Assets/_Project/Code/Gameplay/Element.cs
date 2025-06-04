using DG.Tweening;
using Project.Core;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Project.Gameplay
{
    [RequireComponent(typeof(Animator))]
    public class Element : MonoBehaviour, IElement
    {
        private const string DESTROY_ANIMATION_STATE_NAME = "Destroy";
        private Animator _animator;
        private bool _isDestroyed;
        private CancellationTokenSource _destroyToken;
        private Tween _moveTween;
        public string Name { get; private set; }

        private void Awake() =>
            _animator = GetComponent<Animator>();

        public void OnSwipe(ISwipeData data) { }

        public void SetSiblingIndex(int index)
        {
            if (_isDestroyed)
                return;

            transform.SetSiblingIndex(index);
        }

        public Tween Move(Vector3 position, float duration)
        {
            if (_isDestroyed)
                return DOTween.Sequence();

            KillTween(ref _moveTween);

            position.z = transform.position.z;
            _moveTween = transform.DOMove(position, duration);
            return _moveTween;
        }

        public async void Destroy(float delay = 0f)
        {
            if (_isDestroyed)
                return;

            if (delay > 0f)
            {
                _destroyToken = new CancellationTokenSource();
                await DestroyAsync(delay, _destroyToken.Token);
            }
            
            _destroyToken?.Cancel();
            if (_isDestroyed)
                return;

            Destroy(gameObject);
            KillTween(ref _moveTween);
        }


        private async Task DestroyAsync(float delay, CancellationToken token)
        {
            _animator.Play(DESTROY_ANIMATION_STATE_NAME);

            while (!token.IsCancellationRequested && !GetMainAnimatorStateInfo().IsName(DESTROY_ANIMATION_STATE_NAME))
                await Task.Yield();

            if (token.IsCancellationRequested)
                return;

            float animationLength = GetCurrentClipInfo().clip.length;
            _animator.speed = animationLength / _animator.GetCurrentAnimatorStateInfo(0).speed / delay;

            while (!token.IsCancellationRequested && GetMainAnimatorStateInfo().normalizedTime < .99f)
                await Task.Yield();
        }

        private AnimatorStateInfo GetMainAnimatorStateInfo() =>
            _isDestroyed ? default : _animator.GetCurrentAnimatorStateInfo(0);

        private AnimatorClipInfo GetCurrentClipInfo() =>
            _isDestroyed ? default : _animator.GetCurrentAnimatorClipInfo(0)[0];

        public void Initialize(string name)=> 
            Name = name;

        private void OnDestroy()
        {
            _isDestroyed = true;
            KillTween(ref _moveTween);
        }

        private void KillTween(ref Tween tween)
        {
            if (tween == null)
                return;

            tween.Kill();
            tween = null;
        }
    }
}
