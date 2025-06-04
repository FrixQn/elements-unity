using Project.Core;
using Project.Gameplay;
using Project.UI;
using System;
using VContainer;
using VContainer.Unity;

namespace Project.Presenters
{
    public class GamePresenter : IInitializable, IDisposable
    {
        private const string LEVEL_SAVE_KEY = "Level";
        public readonly GameplayController _gameplay;
        public readonly GameView _view;
        public readonly ISaveSystem _saveSystem;

        [Inject]
        public GamePresenter(ISaveSystem saveSystem, GameplayController gameplay, GameView view)
        {
            _saveSystem = saveSystem;
            _gameplay = gameplay;
            _view = view;

            Subscribe();
        }

        public void Initialize()
        {
            int currentLevel = _saveSystem.GetData<int>(LEVEL_SAVE_KEY);
            _gameplay.StartGame(currentLevel, true);
        }

        private void Subscribe()
        {
            _gameplay.LevelCompleted += OnLevelCompleted;

            _view.NextButtonClicked += NextButtonClicked;
            _view.RestartButtonClicked += RestartButtonClicked;
        }

        private void OnLevelCompleted()
        {
            PlayNextLevel();
        }

        private void RestartButtonClicked()
        {
            _gameplay.RestartCurrentLevel();
        }

        private void NextButtonClicked()
        {
            PlayNextLevel();
        }

        private void PlayNextLevel()
        {
            int nextLevel = _saveSystem.GetData<int>(LEVEL_SAVE_KEY) + 1;
            _saveSystem.WriteData(LEVEL_SAVE_KEY, nextLevel);
            _gameplay.StartGame(nextLevel);
        }

        public void Dispose() =>
            UnSubscribe();

        private void UnSubscribe()
        {
            _gameplay.LevelCompleted -= OnLevelCompleted;

            _view.NextButtonClicked -= NextButtonClicked;
            _view.RestartButtonClicked -= RestartButtonClicked;
        }
    }
}
