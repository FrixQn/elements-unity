using Project.Core;
using Project.Gameplay;
using Project.Presenters;
using Project.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Scopes
{
    public class GameScope : LifetimeScope
    {
        [SerializeField] private SwipesManager _swipesManager;
        [SerializeField] private ElementsSpawner _spawner;
        [SerializeField] private GameplayConfig _gameplayConfig;
        [SerializeField] private GridLayoutConfig _gridLayoutConfig;
        [SerializeField] private BalloonControllerConfig _balloonControllerConfig;
        [SerializeField] private GameView _gameView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SaveSystem>(Lifetime.Scoped).As<ISaveSystem>();

            builder.RegisterInstance(_swipesManager).As<ISwipesManager>();
            builder.RegisterEntryPoint<BalloonController>().WithParameter(Camera.main).
                WithParameter(_balloonControllerConfig);

            builder.RegisterInstance(_gridLayoutConfig);
            builder.Register<GridLayoutService>(Lifetime.Scoped).As<GridLayoutService>().
                WithParameter(Camera.main);

            builder.RegisterEntryPoint<GamePresenter>(Lifetime.Scoped);

            builder.RegisterEntryPoint<ElementsSpawner>().AsSelf();
            builder.Register<GameplayController>(Lifetime.Scoped).WithParameter(_gameplayConfig);

            builder.RegisterInstance(_gameView);

        }
    }
}
