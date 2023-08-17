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
            var levelPrefab = GameObject.Instantiate(_config.levelViewPrefab);
            _floor0Frame = levelPrefab.Floor0Frame;
            _floor0Interactive = levelPrefab.Floor0Interactive;
            _floor1Frame = levelPrefab.Floor1Frame;

            // Wait for tilemap GameObjects be spawned.
            yield return null;
            
            _buildings = new List<BuildingData>();
            BuildingUtil.DetectBuildings(_floor0Frame, _floor0Interactive, _floor1Frame, _buildings);

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
        }

        public bool HasWallAtCell(Vector3Int cell)
        {
            var gameObject = _floor0Frame.GetInstantiatedObject(cell);
            if (gameObject == null)
            {
                return false;
            }

            var structure = gameObject.GetComponentInChildren<BuildingPartView>();
            return structure.PartType == EBuildingPartType.Wall;
        }

        public void OnPlayerBoundsUpdate(Bounds playerBounds)
        {
            var cameraDir = _cameraController.CameraDirection;
            
            var playerBottomCenter = playerBounds.center;
            playerBottomCenter.y = playerBounds.min.y;
            
            var playerBottom0 = playerBounds.min;
            var playerBottom1 = new Vector3(playerBounds.max.x, playerBounds.min.y, playerBounds.min.z);
            var playerBottom2 = new Vector3(playerBounds.min.x, playerBounds.min.y, playerBounds.max.z);
            var playerBottom3 = new Vector3(playerBounds.max.x, playerBounds.min.y, playerBounds.max.z);
            
            var playerBottomRay0 = new Ray(playerBottom0, -cameraDir);
            var playerBottomRay1 = new Ray(playerBottom1, -cameraDir);
            var playerBottomRay2 = new Ray(playerBottom2, -cameraDir);
            var playerBottomRay3 = new Ray(playerBottom3, -cameraDir);
            
            var playerTop0 = playerBounds.max;
            var playerTop1 = new Vector3(playerBounds.min.x, playerBounds.max.y, playerBounds.min.z);
            var playerTop2 = new Vector3(playerBounds.min.x, playerBounds.max.y, playerBounds.max.z);
            var playerTop3 = new Vector3(playerBounds.max.x, playerBounds.max.y, playerBounds.min.z);
            var playerTopRay0 = new Ray(playerTop0, cameraDir);
            var playerTopRay1 = new Ray(playerTop1, cameraDir);
            var playerTopRay2 = new Ray(playerTop2, cameraDir);
            var playerTopRay3 = new Ray(playerTop3, cameraDir);

            for (var i = 0; i < _buildings.Count; i++)
            {
                var blocksView = false;
                foreach (var room in _buildings[i].rooms)
                {
                    var roomBounds = RoomBoundsToFloat(room);
                    
                    if (roomBounds.IntersectRay(playerBottomRay0) ||
                        roomBounds.IntersectRay(playerBottomRay1) ||
                        roomBounds.IntersectRay(playerBottomRay2) ||
                        roomBounds.IntersectRay(playerBottomRay3))
                    {
                        blocksView = true;
                        break;
                    }

                    var extendedRoom = room;
                    extendedRoom.position -= new Vector3Int(1, 1, 0);
                    extendedRoom.size += new Vector3Int(2, 2, 0);
                    roomBounds = RoomBoundsToFloat(extendedRoom);
                    if (roomBounds.Contains(playerBottomCenter))
                    {
                        blocksView = true;
                        break;
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
                }

                var transition = _transitions[i];
        
                transition.targetAlpha = blocksView ? 0 : 1;
            }
        }

        public void Update()
        {
            for (var i = 0; i < _buildings.Count; i++)
            {
                var transition = _transitions[i];

                if (transition.alpha > transition.targetAlpha || transition.alpha < transition.targetAlpha)
                {
                    transition.alpha = Mathf.MoveTowards(transition.alpha,
                        transition.targetAlpha,
                        Time.deltaTime / _config.transitionDuration);
                    
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

#pragma warning disable CS0162
        public void OnDrawGizmos()
        {
            return;
            
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
#pragma warning restore CS0162
        
    }
}