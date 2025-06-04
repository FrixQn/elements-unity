using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    public class GameView : BaseView
    {
        [SerializeField] private Button _restart;
        [SerializeField] private Button _next;

        public event Action RestartButtonClicked;
        public event Action NextButtonClicked;

        public override void Subscribe()
        {
            _restart.onClick.AddListener(OnRestartButtonClicked);
            _next.onClick.AddListener(OnNextButtonClicked);
        }

        private void OnNextButtonClicked()
        {
            NextButtonClicked?.Invoke();
        }

        private void OnRestartButtonClicked()
        {
            RestartButtonClicked?.Invoke();
        }

        public override void Unsubscribe()
        {
            _restart.onClick.RemoveAllListeners();
            _next.onClick.RemoveAllListeners();
        }
    }
}
