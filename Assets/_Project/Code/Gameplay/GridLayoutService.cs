using UnityEngine;
using VContainer;

namespace Project.Gameplay
{
    public class GridLayoutService
    {
        private readonly Camera _camera;
        private readonly GridLayoutConfig _config;

        [Inject]
        public GridLayoutService(GridLayoutConfig config, Camera camera)
        {
            _camera = camera;
            _config = config;
        }

        public (float gridScale, float totalGridWidth, float totalGridHeight, int totalCellsX, Vector3 elementSize) CalculateGridScale(int width, int height)
        {
            int totalCellsX = width + _config.LeftOffsetCells + _config.RightOffsetCells;
            float totalGridWidth = totalCellsX * _config.ElementSize.x + (totalCellsX - 1) * _config.GapX;
            float totalGridHeight = height * _config.ElementSize.y + (height - 1) * _config.GapY;

            float camHeight = _camera.orthographicSize * 2f;
            float camWidth = camHeight * _camera.aspect;

            float gridScale = 1f;
            if (totalGridWidth > camWidth || totalGridHeight > camHeight)
            {
                gridScale = Mathf.Min(camWidth / totalGridWidth, camHeight / totalGridHeight);
            }

            return (gridScale, totalGridWidth, totalGridHeight, totalCellsX, _config.ElementSize);
        }

        public Vector3[,] BuildGrid(int width, int height, Vector3? origin = null)
        {
            var (gridScale, totalGridWidth, totalGridHeight, _, _) = CalculateGridScale(width, height);

            if (origin == null)
            {
                Vector3 cameraOrigin = _camera.transform.position;
                cameraOrigin.z = 0f;
                origin = cameraOrigin;
            }

            float offsetX = -totalGridWidth * gridScale / 2f + _config.ElementSize.x * gridScale / 2f;
            float offsetY = -totalGridHeight * gridScale / 2f + _config.ElementSize.y * gridScale / 2f;

            Vector3[,] positions = new Vector3[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float posX = offsetX + (x + _config.LeftOffsetCells) * (_config.ElementSize.x * gridScale + _config.GapX * gridScale);
                    float posY = offsetY + y * (_config.ElementSize.y * gridScale + _config.GapY * gridScale);
                    positions[x, y] = origin.Value + new Vector3(posX, posY, 0f);
                }
            }
            return positions;
        }
    }
}
