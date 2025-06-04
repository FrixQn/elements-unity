using UnityEngine;
using UnityEngine.Rendering;
using VContainer;

namespace Project.Gameplay
{
    public class ElementsSpawner 
    {
        private const string GAME_OBJECT_NAME = nameof(ElementsSpawner);
        private const int SORTING_ORDER_INDEX = 1;
        private Transform _instance;
        private GridLayoutService _gridLayoutService;

        [Inject]
        private void Inject(GridLayoutService gridLayoutService)
        {
            _gridLayoutService = gridLayoutService;
            CreateGameObjectInstance();
        }

        private void CreateGameObjectInstance()
        {
            var go = new GameObject(GAME_OBJECT_NAME);
            _instance = go.transform;
            go.AddComponent<SortingGroup>().sortingOrder = SORTING_ORDER_INDEX;
        }

        public IElement[] SpawnElements(LevelConfig config, Vector3? origin = null)
        {
            int gridWidth = config.Width;
            int gridHeight = config.Height;
            IElement[] elements = new IElement[gridWidth * gridHeight];
            Vector3[,] positions = _gridLayoutService.BuildGrid(gridWidth, gridHeight, origin);

            var (gridScale, _, _, _, elementSize) = _gridLayoutService.CalculateGridScale(gridWidth, gridHeight);

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    ElementConfig elementConfig = config.GetElementConfig(x, y);
                    if (elementConfig == null)
                        continue;

                    if (elementConfig.Element == null)
                        continue;

                    int index = x * gridHeight + y;
                    var element = CreateElementInstance(elementConfig.Element, elementConfig.name, positions[x, y], Quaternion.identity, out elements[index]);

                    FitElementToGrid(element, elementSize, gridScale);
                }
            }

            return elements;
        }

        public IElement[] SpawnElements(ITileInfo[] tileInfo, LevelConfig config, Vector3? origin = null)
        {
            int gridWidth = config.Width;
            int gridHeight = config.Height;
            IElement[] elements = new IElement[tileInfo.Length];
            Vector3[,] positions = _gridLayoutService.BuildGrid(gridWidth, gridHeight, origin);

            var (gridScale, _, _, _, elementSize) = _gridLayoutService.CalculateGridScale(gridWidth, gridHeight);

            int index;
            foreach(var info in tileInfo)
            {
                index = info.Position.x * gridHeight + info.Position.y;
                ElementConfig elementConfig = config.GetElementConfigByElementName(info.Element);
                if (elementConfig == null)
                    continue;

                if (elementConfig.Element == null)
                    continue;

                var element = CreateElementInstance(elementConfig.Element, elementConfig.name, positions[info.Position.x, info.Position.y], Quaternion.identity, out elements[index]);

                FitElementToGrid(element, elementSize, gridScale);
            }

            return elements;
        }

        private Element CreateElementInstance(Element prefab, string name, Vector3 position, Quaternion rotation, out IElement element)
        {
            var instance = Object.Instantiate(prefab, position, rotation, _instance.transform);
            instance.Initialize(name);
            instance.TryGetComponent(out element);

            return instance;
        }

        private void FitElementToGrid(Element element, Vector2 elementSize, float gridScale)
        {
            SpriteRenderer elementSpriteRenderer = element.GetComponent<SpriteRenderer>();
            if (elementSpriteRenderer != null && elementSpriteRenderer.sprite != null)
            {
                Vector2 spriteSize = elementSpriteRenderer.sprite.bounds.size;
                float fitScale = Mathf.Min(elementSize.x / spriteSize.x, elementSize.y / spriteSize.y);
                element.transform.localScale = fitScale * gridScale * Vector3.one;
            }
            else
            {
                element.transform.localScale = Vector3.one * gridScale;
            }
        }
    }
}
