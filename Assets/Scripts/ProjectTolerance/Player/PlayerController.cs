using System.Collections;
using ProjectTolerance.Cinematics;
using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerController : IUnorderedUpdatable
    {
        public Bounds ViewBounds
        {
            get
            {
                var bounds = _playerView.ColliderBounds;
                bounds.center += CellToWorldPosition(_playerPosition) - _playerView.Position;
                return bounds;
            }
        }
        
        private readonly PlayerConfig _config;
        private readonly PlayerInputController _input;
        private readonly CameraController _camera;

        private PlayerView _playerView;
        private AimSquareView _aimView;
        private Vector2Int _playerPosition;
        private Vector3 _targetAimPosition;

        public PlayerController (PlayerConfig config, PlayerInputController input, CameraController camera)
        {
            _config = config;
            _input = input;
            _camera = camera;
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

            if (inputState.meleeAim && animationFinished)
            {
                var aimDelta = AimDirectionToCellDelta(inputState.meleeAimDirection);
                var targetPosition = _playerPosition + aimDelta;
                _targetAimPosition = CellToWorldPosition(targetPosition);

                if (inputState.meleeAttack)
                {
                    _playerPosition = targetPosition;
                }
            }
            else
            {
                _targetAimPosition = CellToWorldPosition(_playerPosition);
            }

            _playerView.Position = Vector3.MoveTowards(_playerView.Position, CellToWorldPosition(_playerPosition), Time.deltaTime * _config.playerSpeed);
            _aimView.Position = Vector3.MoveTowards(_aimView.Position, _targetAimPosition, Time.deltaTime * _config.aimSpeed);
            
            _camera.SetTargetPosition(_playerView.Position);
        }

        private Vector2Int AimDirectionToCellDelta(Vector2 aimDirection)
        {
            const float QuaterPI = Mathf.PI * 0.25f;
            
            var angle = Mathf.Round(Mathf.Atan2(aimDirection.y, aimDirection.x) / QuaterPI) * QuaterPI + QuaterPI;
            aimDirection = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
            
            return new Vector2Int((int) Mathf.Round(aimDirection.x), (int) Mathf.Round(aimDirection.y));
        }

        private Vector3 CellToWorldPosition(Vector2Int cellPosition)
        {
            return new Vector3(cellPosition.x + 0.5f, 0, cellPosition.y + 0.5f);
        }
    }
}