using System.Collections;
using ProjectTolerance.Buildings;
using ProjectTolerance.Cinematics;
using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerController : IUnorderedUpdatable
    {
        private static readonly Vector3 CellSize = new Vector3(1, 0, 1);
        
        private readonly PlayerConfig _config;
        private readonly PlayerInputController _input;
        private readonly CameraController _camera;
        private readonly BuildingController _buildingController;

        private PlayerView _playerView;
        private AimSquareView _aimView;
        private Vector2Int _playerPosition;
        private Vector3 _targetAimPosition;

        public PlayerController (PlayerConfig config, 
                                 PlayerInputController input, 
                                 CameraController camera,
                                 BuildingController buildingController)
        {
            _config = config;
            _input = input;
            _camera = camera;
            _buildingController = buildingController;
        }

        public IEnumerator Initialize()
        {
            _playerView = GameObject.Instantiate(_config.playerViewPrefab);
            _aimView = GameObject.Instantiate(_config.aimSquareViewPrefab);

            yield break;
        }

        public void Update()
        {
            var inputState = _input.State;

            var targetWorldPosition = CellToWorldPosition(_playerPosition);
            var animationFinished = Vector3.Distance(_playerView.Position, targetWorldPosition) < 0.01f;

            if (inputState.closeAim && animationFinished)
            {
                var aimDelta = AimDirectionToCellDelta(inputState.closeAimDirection);
                var targetPosition = _playerPosition + aimDelta;
                var hasWallAtTarget = _buildingController.HasWallAtCell(new Vector3Int(targetPosition.x, targetPosition.y));
                _targetAimPosition = CellToWorldPosition(targetPosition);
                _aimView.SetVisible(true);
                _aimView.SetBlock(hasWallAtTarget);

                if (inputState.movement && !hasWallAtTarget)
                {
                    _playerPosition = targetPosition;
                    
                    var bounds = _playerView.ColliderBounds;
                    bounds.center += CellToWorldPosition(_playerPosition) - _playerView.Position;
                    _buildingController.OnPlayerBoundsUpdate(bounds);
                }
            }
            else
            {
                _targetAimPosition = CellToWorldPosition(_playerPosition);
                _aimView.SetVisible(false);
                _aimView.SetBlock(false);
            }

            _playerView.Position = Vector3.MoveTowards(_playerView.Position, CellToWorldPosition(_playerPosition), Time.deltaTime * _config.playerSpeed);
            _aimView.Position = Vector3.MoveTowards(_aimView.Position, _targetAimPosition, Time.deltaTime * _config.aimSpeed);
            
            _camera.SetTargetPosition(_playerView.Position);
        }

        private Vector2Int AimDirectionToCellDelta(Vector2 aimDirection)
        {
            const float HalfPI = Mathf.PI * 0.5f;
            
            var angle = Mathf.Round(Mathf.Atan2(aimDirection.y, aimDirection.x) / HalfPI) * HalfPI;
            aimDirection = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
            
            return new Vector2Int((int) Mathf.Round(aimDirection.x), (int) Mathf.Round(aimDirection.y));
        }

        private Vector3 CellToWorldPosition(Vector2Int cellPosition)
        {
            return Vector3.Scale(new Vector3(cellPosition.x + 0.5f, 0, cellPosition.y + 0.5f), CellSize);
        }
    }
}