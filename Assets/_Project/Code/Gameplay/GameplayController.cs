using DG.Tweening;
using Project.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Project.Gameplay
{ 
    public class GameplayController : IDisposable
    {
        private const string LEVEL_STATE_SAVE_KEY = "LevelState";

        private readonly ISwipesManager _swipesManager;
        private readonly GameplayConfig _gameplayConfig;
        private readonly ElementsSpawner _spawner;
        private readonly GridLayoutService _gridLoyoutService;
        private readonly ISaveSystem _saveSystem;
        private CancellationTokenSource _swipeMethodSource;
        private List<IElement> _lockedElements = new();
        private List<Vector2Int> _lockedPositions = new();
        private List<Sequence> _sequences = new();
        private Grid _grid;
        private bool _isBisy;
        private LevelConfig _levelConfig;
        private int _level;

        public event Action LevelCompleted;
        
        [Inject]
        public GameplayController(ISwipesManager swipesManager, ISaveSystem saveSystem, ElementsSpawner spawner, GameplayConfig config, 
            GridLayoutService gridLayoutService)
        {
            _saveSystem = saveSystem;
            _swipesManager = swipesManager;
            _gameplayConfig = config;
            _spawner = spawner;
            _gridLoyoutService = gridLayoutService;

            _swipesManager.SwipeDetected += OnSwipeDetected;

            Application.targetFrameRate = _gameplayConfig.TargetFramerate;
        }

        public void StartGame(int level, bool restoreState = false)
        {
            EndGame();

            if (restoreState && _saveSystem.HasKey(LEVEL_STATE_SAVE_KEY))
            {
                RestoreLevelState();
            }
            else
            {
                InitializeNewLevel(level);
                SaveLevelState();
            }
        }

        private void InitializeNewLevel(int level)
        {
            SetupLevel(level);

            _grid = new Grid(_levelConfig.Width, _levelConfig.Height, GetWorldPositions(), _spawner.SpawnElements(_levelConfig));
        }

        private void RestoreLevelState()
        {
            var state = _saveSystem.GetData<LevelState>(LEVEL_STATE_SAVE_KEY);
            SetupLevel(state.Level);

            var elements = _spawner.SpawnElements(state.Tiles.Select(t => t as ITileInfo).ToArray(), _levelConfig);
            _grid = new Grid(_levelConfig.Width, _levelConfig.Height, GetWorldPositions(), elements);
        }

        private void SetupLevel(int level)
        {
            _level = level;
            _levelConfig = _gameplayConfig.GetLevelConfig(level);
        }

        private Vector3[,] GetWorldPositions()
        {
            return _gridLoyoutService.BuildGrid(_levelConfig.Width, _levelConfig.Height);
        }

        private void SaveLevelState()
        {
            LevelState state = new()
            {
                Level = _level
            };
            var tiles = _grid.GetTiles();
            state.Tiles = new TileInfo[tiles.Length];
            for (int i = 0; i < tiles.Length; i++)
            {
                state.Tiles[i] = new TileInfo() { Position = tiles[i].Position, Element = tiles[i].Element?.Name };
            }

            _saveSystem.WriteData(LEVEL_STATE_SAVE_KEY, state);
        }

        private void RemoveGameState() => 
            _saveSystem.DeleteKey(LEVEL_STATE_SAVE_KEY);

        public void RestartCurrentLevel()
        {
            RemoveGameState();
            StartGame(_level);
        }

        private void EndGame()
        {
            _swipeMethodSource?.Cancel();

            foreach (var sequence in _sequences)
            {
                sequence.Kill();
            }
            _sequences.Clear();

            var tiles = _grid.GetTiles();

            foreach (var tile in tiles)
            {
                tile.Element?.Destroy();
            }

            foreach(var element in _lockedElements)
            {
                element?.Destroy();
            }

            ClearLocks();
        }

        private async void OnSwipeDetected(ISwipeData swipeData)
        {
            if (swipeData.Target is not IElement element)
                return;

            float duration = _gameplayConfig.AnimationsDuration;
            ITile tile = _grid.GetElementTile(element);
            if (tile == null)
                return;

            ITile neighbour = _grid.GetNeighbourTile(tile, GetNeighbourDirection(swipeData.Direction));
            if (neighbour == null)
                return;

            if (_isBisy)
                return;

            _isBisy = true;
            _swipeMethodSource = new CancellationTokenSource();

            bool swipeAccepted = TryAnimateSwap(tile, neighbour, swipeData.Direction, duration, out Sequence swapSequence);

            if (swipeAccepted)
            {
                try
                {
                    _grid.SwapTiles(tile.Position, neighbour.Position);
                    _sequences.Add(swapSequence);
                    SaveLevelState();
                    await Task.Run(swapSequence.AsyncWaitForCompletion, _swipeMethodSource.Token);

                    await NormalizeGrid(duration, _swipeMethodSource.Token);
                    SaveLevelState();

                    await ProcessMatchesAsync(duration, _swipeMethodSource.Token);
                }
                catch (Exception e)
                {
                    if (e is TaskCanceledException)
                    {
                        ClearLocks();
                        ClearInactiveSequences();
                        ProcessLevelCompletion();
                    }
                }
            }

            _isBisy = false;
        }


        private bool TryAnimateSwap(ITile origin, ITile neighbour, SwipeDirection direction, float duration, out Sequence sequence)
        {
            sequence = DOTween.Sequence();

            if (IsElementLocked(origin.Element) || IsElementLocked(neighbour.Element) || 
                IsPositionLocked(origin.Position) || IsPositionLocked(neighbour.Position))
            {
                return false;
            }

            bool hasNeighbourElement = neighbour.Element != null;
            bool canMoveWithoutNeighbour = !hasNeighbourElement && direction != SwipeDirection.Up;

            if (!hasNeighbourElement && !canMoveWithoutNeighbour)
                return false;

            LockTile(origin);
            LockTile(neighbour);
            SetSiblingIndex(origin.Element, neighbour.Position);

            if (hasNeighbourElement)
            {
                SetSiblingIndex(neighbour.Element, origin.Position);
                sequence.Insert(0f, origin.Element?.Move(neighbour.WorldPosition, duration))
                        .Insert(0f, neighbour.Element?.Move(origin.WorldPosition, duration));
            }
            else
            {
                sequence.Append(origin.Element?.Move(neighbour.WorldPosition, duration));
            }

            sequence.OnComplete(() => OnSwapSequenceCompleted(origin, neighbour));

            return true;
        }

        private void OnSwapSequenceCompleted(ITile origin, ITile neighbour)
        {
            UnlockTile(origin);
            UnlockTile(neighbour);
        }

        private Sequence CreateDropSequence(List<TileMoveInfo> moveInfo, float duration)
        {
            var dropSequence = DOTween.Sequence();
            foreach (var info in moveInfo)
            {
                LockElement(info.Element);
                LockPosition(info.To);
                info.Element?.SetSiblingIndex(CalculateSiblingIndexByPosition(info.To));
                dropSequence.Insert(0, info.Element?.Move(info.ToWorldPosition, duration));
            }

            foreach(var tile in _grid.GetTiles())
            {
                if (!tile.IsEmpty)
                    tile.Element.SetSiblingIndex(CalculateSiblingIndexByPosition(tile.Position));
            }

            return dropSequence;
        }

        private async Task ProcessMatchesAsync(float duration, CancellationToken cancellationToken)
        {
            float delay;
            List<ITile> tiles = new ();
            while (!cancellationToken.IsCancellationRequested)
            {
                var groups = _grid.FindAllConnectedGroups();
                if (groups.Count == 0)
                    break;

                delay = duration;
                foreach (var group in groups)
                {
                    foreach (var tile in group)
                    {
                        LockTile(tile);
                        tiles.Add(tile);
                        tile.Element.Destroy(duration);
                        _grid.ClearTile(tile);
                    }
                }

                while(delay > 0 && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Yield();
                    delay -= Time.deltaTime;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    ForceDestroyElements(tiles);
                    tiles.Clear();
                    break;
                }

                ClearLocks();
                tiles.Clear();

                await NormalizeGrid(duration, cancellationToken);
                SaveLevelState();
            }

            SaveLevelState();
            ClearLocks();
            ClearInactiveSequences();
            ProcessLevelCompletion();
        }

        private void ProcessLevelCompletion()
        {
            if (!_grid.IsEmpty())
                return;

            RemoveGameState();
            LevelCompleted?.Invoke();
        }

        public Task NormalizeGrid(float duration, CancellationToken cancellationToken)
        {
            var dropInfo = _grid.NormalizeGrid();
            var dropSequence = CreateDropSequence(dropInfo, duration);
            _sequences.Add(dropSequence);
            return Task.Run(dropSequence.AsyncWaitForCompletion, cancellationToken);
        }

        private void SetSiblingIndex(IElement element, Vector2Int position)
        {
            if (element == null)
                return;

            element.SetSiblingIndex(CalculateSiblingIndexByPosition(position));
        }

        private void ClearInactiveSequences() =>
            _sequences.RemoveAll(s => !s.active);

        private void ForceDestroyElements(List<ITile> tiles)
        {
            foreach (var tile in tiles)
            {
                tile.Element?.Destroy();
            }
        }

        private bool IsElementLocked(IElement element) =>
            _lockedElements.Contains(element);

        private bool IsPositionLocked(Vector2Int position) =>
            _lockedPositions.Contains(position);

        private void LockTile(ITile tile)
        {
            LockElement(tile.Element);
            LockPosition(tile.Position);
        }

        private void UnlockTile(ITile tile)
        {
            UnlockElement(tile.Element);
            UnlockPosition(tile.Position);
        }

        private void LockPosition(Vector2Int position) =>
            _lockedPositions.Add(position);

        private void UnlockPosition(Vector2Int position) =>
            _lockedPositions.Remove(position);

        private void LockElement(IElement element) =>
            _lockedElements.Add(element);

        private void UnlockElement(IElement element) =>
            _lockedElements.Remove(element);

        private void ClearLocks()
        {
            _lockedElements.Clear();
            _lockedPositions.Clear();
        }

        private Grid.NeighbourDir GetNeighbourDirection(SwipeDirection dir)
        {
            return dir switch
            {
                SwipeDirection.Up => Grid.NeighbourDir.Top,
                SwipeDirection.Left => Grid.NeighbourDir.Left,
                SwipeDirection.Right => Grid.NeighbourDir.Right,
                SwipeDirection.Down => Grid.NeighbourDir.Bottom,
            };
        }

        private int CalculateSiblingIndexByPosition(Vector2Int position)
        {
            return position.y * _levelConfig.Width + position.x;
        }

        public void Dispose()
        {
            _swipesManager.SwipeDetected -= OnSwipeDetected;
        }
    }
}
