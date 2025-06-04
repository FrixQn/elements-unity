using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Project.Core 
{
    public class BalloonController : ITickable
    {
        public enum MoveDirection
        {
            Left,
            Right
        }

        private readonly Camera _camera;
        private readonly BalloonControllerConfig _config;
        private float spawnTimer = 0f;
        private List<Balloon> _balloons = new();

        [Inject]
        public BalloonController(Camera camera, BalloonControllerConfig config)
        {
            _camera = camera;
            _config = config;
        }

        public void Tick()
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= _config.SpawnInterval && _balloons.Count < _config.BallonsLimit)
            {
                SpawnPrefab();
                spawnTimer = 0f;
            }


            foreach(var balloon in _balloons)
            {
                balloon.Tick();
            }

            int index = 0;
            while(index < _balloons.Count)
            {
                if (IsOutsideOfCamera(_balloons[index]))
                {
                    _balloons[index].Destroy();
                    _balloons.RemoveAt(index);
                }
                else
                    index++;
            }
            
        }

        void SpawnPrefab()
        {
            if (_config.Prefabs == null || _config.Prefabs.Length == 0)
            {
                return;
            }

            int index = Random.Range(0, _config.Prefabs.Length);
            Balloon prefab = _config.Prefabs[index];

            MoveDirection moveDirection = GetRandomMoveDirection();
            float speed = Random.Range(_config.MinSpeed, _config.MaxSpeed);
            float amplitude = Random.Range(_config.MinCurveHeight, _config.MaxCurveHeight);
            float frequency = Random.Range(_config.MinFrequency, _config.MaxFrequency);

            Vector3 spawnPos = CalculateSpawnPosition(moveDirection);

            Balloon balloon = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
            balloon.Initialize(CreateMovementFunction(_config.Distance, spawnPos, moveDirection, amplitude, frequency), 
                speed / _config.Distance, moveDirection);

            _balloons.Add(balloon);
        }

        private IMovementFunction CreateMovementFunction(float distance, Vector3 origin, MoveDirection moveDir, float amplitude, float frequency)
        {
            return new SinusMovementFunction(distance, origin, ConvertDirectionToInt(moveDir), amplitude, frequency);
        }

        private Vector3 CalculateSpawnPosition(MoveDirection direction)
        {
            Vector3 spawnPos;
            if (direction == MoveDirection.Right)
            {
                spawnPos = _camera.ViewportToWorldPoint(new Vector3(0, 0.5f, Mathf.Abs(_camera.transform.position.z)));
                spawnPos.x -= _config.SpawnOffset;
            }
            else
            {
                spawnPos = _camera.ViewportToWorldPoint(new Vector3(1, 0.5f, Mathf.Abs(_camera.transform.position.z)));
                spawnPos.x += _config.SpawnOffset;
            }
            spawnPos.z = 0;

            return spawnPos;
        }

        private int ConvertDirectionToInt(MoveDirection dir)
        {
            return dir switch
            {
                MoveDirection.Left => -1,
                MoveDirection.Right => 1,
            };
        }

        private MoveDirection GetRandomMoveDirection() =>
            Random.value < 0.5f ? MoveDirection.Right : MoveDirection.Left;

        private bool IsOutsideOfCamera(Balloon balloon)
        {
            Vector3 viewportPos = _camera.WorldToViewportPoint(balloon.transform.position);
            return (balloon.MoveDirection == MoveDirection.Right && viewportPos.x > 1.0f + (_config.SpawnOffset / _camera.orthographicSize)) ||
                    (balloon.MoveDirection == MoveDirection.Left && viewportPos.x < 0.0f - (_config.SpawnOffset / _camera.orthographicSize));
        }
    }
}