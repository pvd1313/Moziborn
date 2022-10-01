using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectTolerance.Buildings
{
    public class BuildingController : MonoBehaviour
    {
        private class BuildingTransition
        {
            public float targetAlpha;
            public float alpha;
            public MaterialPropertyBlock materialProperty;
        }

        public IReadOnlyList<BuildingData> Buildings => _buildings;

        [SerializeField] private Tilemap _level0Frame;
        [SerializeField] private Tilemap _level1Interactive;
        [SerializeField] private Tilemap _level1Frame;
        [SerializeField] private float _transitionDuration;
        [SerializeField] private Collider _playerCollider;
        [SerializeField] private Transform _cameraTransform;

        private List<BuildingData> _buildings;
        private BuildingTransition[] _transitions;
        private bool _initialized;
        
        public IEnumerator Initialize()
        {
            // Wait for tilemap GameObjects be spawned.
            yield return null;
            
            _buildings = new List<BuildingData>();
            BuildingUtil.DetectBuildings(_level0Frame, _level1Interactive, _level1Frame, _buildings);

            _transitions = new BuildingTransition[_buildings.Count];
            for (var i = 0; i < _transitions.Length; i++)
            {
                var propertyBlock = new MaterialPropertyBlock();
                
                propertyBlock.SetFloat(ShaderPropertyId.VerticalCutAlpha, 1);
                
                _transitions[i] = new BuildingTransition
                {
                    targetAlpha = 1,
                    alpha = 1,
                    materialProperty = propertyBlock
                };

                foreach (var renderer in _buildings[i].viewBlockingParts)
                {
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }

            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            var cameraPos = _cameraTransform.position;
            var playerBounds = _playerCollider.bounds;
            var playerBottomRay = new Ray(cameraPos, playerBounds.min - cameraPos);
            
            var playerTop0 = playerBounds.max;
            var playerTop1 = new Vector3(playerBounds.min.x, playerBounds.max.y, playerBounds.min.z);
            var playerTop2 = new Vector3(playerBounds.min.x, playerBounds.max.y, playerBounds.max.z);
            var playerTop3 = new Vector3(playerBounds.max.x, playerBounds.max.y, playerBounds.min.z);
            var playerTopRay0 = new Ray(cameraPos, playerTop0 - cameraPos);
            var playerTopRay1 = new Ray(cameraPos, playerTop1 - cameraPos);
            var playerTopRay2 = new Ray(cameraPos, playerTop2 - cameraPos);
            var playerTopRay3 = new Ray(cameraPos, playerTop3 - cameraPos);

            for (var i = 0; i < _buildings.Count; i++)
            {
                var blocksView = false;
                foreach (var room in _buildings[i].rooms)
                {
                    if (RoomBoundsToFloat(room).IntersectRay(playerBottomRay))
                    {
                        blocksView = true;
                        break;
                    }
                }

                foreach (var doorFrame in _buildings[i].doorFrames)
                {
                    var doorBounds = DoorFrameToBoundsFloat(doorFrame);
                    
                    if (doorBounds.IntersectRay(playerTopRay0) || 
                        doorBounds.IntersectRay(playerTopRay1) ||
                        doorBounds.IntersectRay(playerTopRay2) ||
                        doorBounds.IntersectRay(playerTopRay3))
                    {
                        blocksView = true;
                        break;
                    }
                }
                
                var transition = _transitions[i];

                transition.targetAlpha = blocksView ? 0 : 1;
                
                if (transition.alpha > transition.targetAlpha || transition.alpha < transition.targetAlpha)
                {
                    transition.alpha = Mathf.MoveTowards(transition.alpha,
                        transition.targetAlpha,
                        Time.deltaTime / _transitionDuration);
                    
                    transition.materialProperty.SetFloat(ShaderPropertyId.VerticalCutAlpha, transition.alpha);

                    foreach (var part in _buildings[i].viewBlockingParts)
                    {
                        part.SetPropertyBlock(transition.materialProperty);
                    }
                }
            }
        }

        private Bounds RoomBoundsToFloat(BoundsInt bounds)
        {
            var center = new Vector3(bounds.center.x, 1.65f, bounds.center.y);
            var size = new Vector3(bounds.size.x, 3.3f, bounds.size.y);
            return new Bounds(center, size);
        }

        private Bounds DoorFrameToBoundsFloat(Vector2Int pos)
        {
            var center = new Vector3(pos.x + 0.5f, 1.5f, pos.y + 0.5f);
            var size = new Vector3(1, 3, 1);
            return new Bounds(center, size);
        }

        private void OnDrawGizmos()
        {
            if (_buildings == null)
            {
                return;
            }

            foreach (var building in _buildings)
            {
                Gizmos.color = Color.yellow;
                foreach (var room in building.rooms)
                {
                    var bounds = RoomBoundsToFloat(room);
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }

                Gizmos.color = Color.cyan;
                foreach (var doorFrame in building.doorFrames)
                {
                    var bounds = DoorFrameToBoundsFloat(doorFrame);
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }
            }
        }
    }
}