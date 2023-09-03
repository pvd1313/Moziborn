using System.Collections;
using System.Collections.Generic;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Debugging;
using ProjectTolerance.GameCycle;
using ProjectTolerance.Shading;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectTolerance.Buildings
{
    public class BuildingController : IUnorderedUpdatable, IGizmosDrawer
    {
        private class BuildingTransition
        {
            public float targetAlpha;
            public float alpha;
            public MaterialPropertyBlock materialProperty;
        }

        private static readonly bool DrawGizmos = false;

        public IReadOnlyList<BuildingData> buildings => _buildings;

        private readonly BuildingConfig _config;
        private readonly MainCameraController _mainCamera;

        private Tilemap _floor0Frame;
        private Tilemap _floor0Interactive;
        private Tilemap _floor1Frame;
        private List<BuildingData> _buildings;
        private BuildingTransition[] _transitions;

        public BuildingController (BuildingConfig config, MainCameraController mainCamera)
        {
            _config = config;
            _mainCamera = mainCamera;
        }
        
        public IEnumerator Initialize()
        {
            LevelView levelPrefab = Object.Instantiate(_config.levelViewPrefab);
            _floor0Frame = levelPrefab.floor0Frame;
            _floor0Interactive = levelPrefab.floor0Interactive;
            _floor1Frame = levelPrefab.floor1Frame;

            // Wait for tilemap GameObjects be spawned.
            yield return null;
            
            _buildings = new List<BuildingData>();
            BuildingUtil.DetectBuildings(_floor0Frame, _floor0Interactive, _floor1Frame, _buildings);

            _transitions = new BuildingTransition[_buildings.Count];
            for (int i = 0; i < _transitions.Length; i++)
            {
                MaterialPropertyBlock propertyBlock = new ();
                
                propertyBlock.SetFloat(ShaderPropertyId.VerticalCutAlpha, 1);
                propertyBlock.SetVector(ShaderPropertyId.VerticalCutRange, _config.verticalCutRange);
                
                _transitions[i] = new BuildingTransition
                {
                    targetAlpha = 1,
                    alpha = 1,
                    materialProperty = propertyBlock
                };

                foreach (MeshRenderer renderer in _buildings[i].viewBlockingParts)
                {
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }

        public void OnPlayerBoundsUpdate(Bounds playerBounds, Vector3 aimDirection)
        {
            const float WallHeight = 3.2f;
            
            Vector3 cameraDir = _mainCamera.forward;

            Vector3 playerBottomCenter = playerBounds.center;
            playerBottomCenter.y = playerBounds.min.y + 0.01f;

            Vector3 playerBottom0 = playerBounds.min;
            Vector3 playerBottom1 = new (playerBounds.max.x, playerBounds.min.y, playerBounds.min.z);
            Vector3 playerBottom2 = new (playerBounds.min.x, playerBounds.min.y, playerBounds.max.z);
            Vector3 playerBottom3 = new (playerBounds.max.x, playerBounds.min.y, playerBounds.max.z);
            
            Ray playerBottomRay0 = new (playerBottom0, -cameraDir);
            Ray playerBottomRay1 = new (playerBottom1, -cameraDir);
            Ray playerBottomRay2 = new (playerBottom2, -cameraDir);
            Ray playerBottomRay3 = new (playerBottom3, -cameraDir);
            
            Vector3 aimRayOffset = Vector3.Cross(aimDirection, Vector3.up) * Mathf.Cos(_mainCamera.rotationDegrees.x * Mathf.Deg2Rad) * WallHeight;
            Ray aimRay0 = new (playerBottomCenter + aimRayOffset, aimDirection);
            Ray aimRay1 = new (playerBottomCenter - aimRayOffset, aimDirection);

            if (DrawGizmos)
            {
                Debug.DrawRay(aimRay0.origin, aimRay0.direction * _mainCamera.orthographicSize, Color.cyan);
                Debug.DrawRay(aimRay1.origin, aimRay1.direction * _mainCamera.orthographicSize, Color.cyan);
            }

            bool playerInsideBuilding = false;

            for (int i = 0; i < _buildings.Count && !playerInsideBuilding; i++)
            {
                foreach (BoundsInt room in buildings[i].rooms)
                {
                    Bounds roomBounds = RoomBoundsToFloat(room);
                    if (roomBounds.Contains(playerBottomCenter))
                    {
                        playerInsideBuilding = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < _buildings.Count; i++)
            {
                bool blocksView = false;
                
                foreach (BoundsInt room in _buildings[i].rooms)
                {
                    Bounds roomBounds = RoomBoundsToFloat(room);
                    
                    if (roomBounds.IntersectRay(playerBottomRay0) ||
                        roomBounds.IntersectRay(playerBottomRay1) ||
                        roomBounds.IntersectRay(playerBottomRay2) ||
                        roomBounds.IntersectRay(playerBottomRay3))
                    {
                        blocksView = true;
                        break;
                    }

                    if (!playerInsideBuilding)
                    {
                        if (roomBounds.IntersectRay(aimRay0) ||
                            roomBounds.IntersectRay(aimRay1))
                        {
                            blocksView = true;
                            break;
                        }
                    }

                    BoundsInt extendedRoom = room;
                    extendedRoom.position -= new Vector3Int(1, 1, 0);
                    extendedRoom.size += new Vector3Int(2, 2, 0);
                    roomBounds = RoomBoundsToFloat(extendedRoom);
                    if (roomBounds.Contains(playerBottomCenter))
                    {
                        blocksView = true;
                        break;
                    }
                }

                BuildingTransition transition = _transitions[i];
        
                transition.targetAlpha = blocksView ? 0 : 1;
            }
        }

        public void Update()
        {
            for (int i = 0; i < _buildings.Count; i++)
            {
                BuildingTransition transition = _transitions[i];

                if (transition.alpha > transition.targetAlpha || transition.alpha < transition.targetAlpha)
                {
                    transition.alpha = Mathf.MoveTowards(transition.alpha,
                        transition.targetAlpha,
                        Time.deltaTime / _config.transitionDuration);
                    
                    transition.materialProperty.SetFloat(ShaderPropertyId.VerticalCutAlpha, transition.alpha);
        
                    foreach (MeshRenderer part in _buildings[i].viewBlockingParts)
                    {
                        part.SetPropertyBlock(transition.materialProperty);
                    }
                }
            }
        }

        private Bounds RoomBoundsToFloat(BoundsInt bounds)
        {
            Vector3 center = new (bounds.center.x, 1.65f, bounds.center.y);
            Vector3 size = new (bounds.size.x, 3.3f, bounds.size.y);
            return new Bounds(center, size);
        }

        public void OnDrawGizmos()
        {
            if (DrawGizmos)
            {
                foreach (BuildingData building in _buildings)
                {
                    Gizmos.color = Color.yellow;
                    foreach (BoundsInt room in building.rooms)
                    {
                        Bounds bounds = RoomBoundsToFloat(room);
                        Gizmos.DrawWireCube(bounds.center, bounds.size);
                    }
                }
            }
        }
    }
}