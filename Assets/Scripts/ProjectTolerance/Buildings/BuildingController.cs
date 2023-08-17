using System.Collections;
using System.Collections.Generic;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Debugging;
using ProjectTolerance.GameCycle;
using ProjectTolerance.Player;
using ProjectTolerance.Shading;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectTolerance.Buildings
{
    public class BuildingController : IUnorderedUpdatable, IGizmosDrawer
    {
        private readonly BuildingConfig _config;

        private class BuildingTransition
        {
            public float targetAlpha;
            public float alpha;
            public MaterialPropertyBlock materialProperty;
        }

        public IReadOnlyList<BuildingData> Buildings => _buildings;

        private readonly CameraController _cameraController;

        private Tilemap _floor0Frame;
        private Tilemap _floor0Interactive;
        private Tilemap _floor1Frame;
        private List<BuildingData> _buildings;
        private BuildingTransition[] _transitions;

        public BuildingController (BuildingConfig config, CameraController cameraController)
        {
            _config = config;
            _cameraController = cameraController;
        }
        
        public IEnumerator Initialize()
        {
            LevelView levelPrefab = GameObject.Instantiate(_config.levelViewPrefab);
            _floor0Frame = levelPrefab.Floor0Frame;
            _floor0Interactive = levelPrefab.Floor0Interactive;
            _floor1Frame = levelPrefab.Floor1Frame;

            // Wait for tilemap GameObjects be spawned.
            yield return null;
            
            _buildings = new List<BuildingData>();
            BuildingUtil.DetectBuildings(_floor0Frame, _floor0Interactive, _floor1Frame, _buildings);

            _transitions = new BuildingTransition[_buildings.Count];
            for (int i = 0; i < _transitions.Length; i++)
            {
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                
                propertyBlock.SetFloat(ShaderPropertyId.VerticalCutAlpha, 1);
                
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

        public bool HasWallAtCell(Vector3Int cell)
        {
            GameObject gameObject = _floor0Frame.GetInstantiatedObject(cell);
            if (gameObject == null)
            {
                return false;
            }

            BuildingPartView structure = gameObject.GetComponentInChildren<BuildingPartView>();
            return structure.PartType == EBuildingPartType.Wall;
        }

        public void OnPlayerBoundsUpdate(Bounds playerBounds)
        {
            Vector3 cameraDir = _cameraController.forward;
            
            Vector3 playerBottomCenter = playerBounds.center;
            playerBottomCenter.y = playerBounds.min.y;
            
            Vector3 playerBottom0 = playerBounds.min;
            Vector3 playerBottom1 = new Vector3(playerBounds.max.x, playerBounds.min.y, playerBounds.min.z);
            Vector3 playerBottom2 = new Vector3(playerBounds.min.x, playerBounds.min.y, playerBounds.max.z);
            Vector3 playerBottom3 = new Vector3(playerBounds.max.x, playerBounds.min.y, playerBounds.max.z);
            
            Ray playerBottomRay0 = new Ray(playerBottom0, -cameraDir);
            Ray playerBottomRay1 = new Ray(playerBottom1, -cameraDir);
            Ray playerBottomRay2 = new Ray(playerBottom2, -cameraDir);
            Ray playerBottomRay3 = new Ray(playerBottom3, -cameraDir);
            
            Vector3 playerTop0 = playerBounds.max;
            Vector3 playerTop1 = new Vector3(playerBounds.min.x, playerBounds.max.y, playerBounds.min.z);
            Vector3 playerTop2 = new Vector3(playerBounds.min.x, playerBounds.max.y, playerBounds.max.z);
            Vector3 playerTop3 = new Vector3(playerBounds.max.x, playerBounds.max.y, playerBounds.min.z);
            Ray playerTopRay0 = new Ray(playerTop0, cameraDir);
            Ray playerTopRay1 = new Ray(playerTop1, cameraDir);
            Ray playerTopRay2 = new Ray(playerTop2, cameraDir);
            Ray playerTopRay3 = new Ray(playerTop3, cameraDir);

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

                    BoundsInt extendedRoom = room;
                    extendedRoom.position -= new Vector3Int(1, 1, 0);
                    extendedRoom.size += new Vector3Int(2, 2, 0);
                    roomBounds = RoomBoundsToFloat(extendedRoom);
                    if (roomBounds.Contains(playerBottomCenter))
                    {
                        blocksView = true;
                        break;
                    }
                    
                    foreach (Vector2Int doorFrame in _buildings[i].doorFrames)
                    {
                        Bounds doorBounds = DoorFrameToBoundsFloat(doorFrame);
                    
                        if (doorBounds.IntersectRay(playerTopRay0) || 
                            doorBounds.IntersectRay(playerTopRay1) ||
                            doorBounds.IntersectRay(playerTopRay2) ||
                            doorBounds.IntersectRay(playerTopRay3))
                        {
                            blocksView = true;
                            break;
                        }
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
            Vector3 center = new Vector3(bounds.center.x, 1.65f, bounds.center.y);
            Vector3 size = new Vector3(bounds.size.x, 3.3f, bounds.size.y);
            return new Bounds(center, size);
        }

        private Bounds DoorFrameToBoundsFloat(Vector2Int pos)
        {
            Vector3 center = new Vector3(pos.x + 0.5f, 1.5f, pos.y + 0.5f);
            Vector3 size = new Vector3(1, 3, 1);
            return new Bounds(center, size);
        }

#pragma warning disable CS0162
        public void OnDrawGizmos()
        {
            return;
            
            foreach (BuildingData building in _buildings)
            {
                Gizmos.color = Color.yellow;
                foreach (BoundsInt room in building.rooms)
                {
                    Bounds bounds = RoomBoundsToFloat(room);
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }

                Gizmos.color = Color.cyan;
                foreach (Vector2Int doorFrame in building.doorFrames)
                {
                    Bounds bounds = DoorFrameToBoundsFloat(doorFrame);
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }
            }
        }
#pragma warning restore CS0162
        
    }
}