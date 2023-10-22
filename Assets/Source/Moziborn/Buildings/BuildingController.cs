using System.Collections;
using System.Collections.Generic;
using Moziborn.Cinematics;
using Moziborn.Debugging;
using Moziborn.Rendering;
using UnityEngine;
using UnityEngine.Tilemaps;

using Object = UnityEngine.Object;

namespace Moziborn.Buildings
{
    public class BuildingController : IGizmosDrawer, ILevelLayout
    {
        private class BuildingTransition
        {
            public float targetAlpha;
            public float alpha;
        }

        private static readonly bool DrawGizmos = true;

        public IReadOnlyList<BuildingData> buildings => _buildings;
        
        public BoundsInt bounds { get; private set; }
        
        public int currentBuildingIndex { get; private set; }

        private readonly BuildingConfig _config;
        private readonly MainCameraController _mainCamera;

        private Tilemap _floor0Frame;
        private Tilemap _floor0Interactive;
        private Tilemap _floor1Frame;
        private List<BuildingData> _buildings;
        private BuildingTransition[] _transitions;
        private MaterialPropertyBlock _materialPropertyBlock;
        private MaterialPropertyBlock _roofMaterialProperty;

        public BuildingController (BuildingConfig config, MainCameraController mainCamera)
        {
            _config = config;
            _mainCamera = mainCamera;
        }
        
        public IEnumerator Initialize()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
            
            LevelView levelPrefab = Object.Instantiate(_config.levelViewPrefab);
            _floor0Frame = levelPrefab.floor0Frame;
            _floor0Interactive = levelPrefab.floor0Interactive;
            _floor1Frame = levelPrefab.floor1Frame;

            BoundsInt bounds = _floor0Frame.cellBounds;
            bounds = JoinBounds(bounds, _floor0Interactive.cellBounds);
            bounds = JoinBounds(bounds, _floor1Frame.cellBounds);
            this.bounds = bounds;

            // Wait for tilemap GameObjects to be spawned.
            yield return null;
            
            _buildings = new List<BuildingData>();
            BuildingUtil.DetectBuildings(_floor0Frame, _floor0Interactive, _floor1Frame, _buildings);

            MaterialPropertyBlock roofPropertyBlock = new ();
            roofPropertyBlock.SetFloat(ShaderPropertyID.Standard.WarFogGroundRoofBlending, 1);

            foreach (BuildingData building in buildings)
            foreach (BoundsInt room in building.rooms)
            foreach (Vector3Int pos in room.allPositionsWithin)
            {
                GameObject gameObject = _floor1Frame.GetInstantiatedObject(pos);
                if (gameObject != null)
                {
                    BuildingPartView partView = gameObject.GetComponent<BuildingPartView>();
                    
                    if (partView.partType == BuildingPartType.Roof)
                    {
                        MeshRenderer meshRenderer = partView.GetComponent<MeshRenderer>();
                        if (meshRenderer != null)
                        {
                            meshRenderer.SetPropertyBlock(roofPropertyBlock);
                        }
                    }
                }
            }

            _transitions = new BuildingTransition[_buildings.Count];
            for (int i = 0; i < _transitions.Length; i++)
            {
                _transitions[i] = new BuildingTransition
                {
                    targetAlpha = 1,
                    alpha = 1
                };
            }

            Update(true);
        }

        public void UpdateViewerState(Bounds playerBounds, Vector3 aimDirection)
        {
            Vector3 cameraDir = _mainCamera.lookDirection;

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

            Vector3 aimRayOffset = Vector3.Cross(aimDirection, Vector3.up) *
                                   (Mathf.Cos(_mainCamera.rotationDeg.x * Mathf.Deg2Rad) * _config.verticalCutRange.y);
            Ray aimRay0 = new (playerBottomCenter + aimRayOffset, aimDirection);
            Ray aimRay1 = new (playerBottomCenter - aimRayOffset, aimDirection);

            if (DrawGizmos)
            {
                Debug.DrawRay(aimRay0.origin, aimRay0.direction * _mainCamera.orthographicSize, Color.cyan);
                Debug.DrawRay(aimRay1.origin, aimRay1.direction * _mainCamera.orthographicSize, Color.cyan);
            }

            currentBuildingIndex = -1;
            
            for (int i = 0; i < _buildings.Count && currentBuildingIndex == -1; i++)
            {
                foreach (BoundsInt room in buildings[i].rooms)
                {
                    Bounds roomBounds = RoomBoundsToFloat(room);
                    if (roomBounds.Contains(playerBottomCenter))
                    {
                        currentBuildingIndex = i;
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

                    if (currentBuildingIndex == -1)
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

        public void Update(bool force)
        {
            _materialPropertyBlock.SetFloat(ShaderPropertyID.Standard.VerticalCutAlpha, 1);
            _materialPropertyBlock.SetVector(ShaderPropertyID.Standard.VerticalCutRange, _config.verticalCutRange);

            float transitionSpeed = Time.deltaTime / _config.transitionDuration;
            
            for (int buildingID = 0; buildingID < _buildings.Count; buildingID++)
            {
                BuildingTransition transition = _transitions[buildingID];

                if (force || transition.alpha > transition.targetAlpha || transition.alpha < transition.targetAlpha)
                {
                    transition.alpha = Mathf.MoveTowards(transition.alpha, transition.targetAlpha, transitionSpeed);
                    
                    _materialPropertyBlock.SetFloat(ShaderPropertyID.Standard.VerticalCutAlpha, transition.alpha);

                    foreach (BuildingPartView partView in _buildings[buildingID].allPartViews)
                    {
                        if (partView.partType == BuildingPartType.Wall ||
                            partView.partType == BuildingPartType.DoorFrame)
                        {
                            _materialPropertyBlock.SetFloat(ShaderPropertyID.Standard.WarFogGroundRoofBlending, 0);
                            for (int i = 0; i < partView.meshRenders.Count; i++)
                            {
                                partView.meshRenders[i].SetPropertyBlock(_materialPropertyBlock);
                            }
                        }
                        else if (partView.partType == BuildingPartType.Roof)
                        {
                            _materialPropertyBlock.SetFloat(ShaderPropertyID.Standard.WarFogGroundRoofBlending, 1);
                            for (int i = 0; i < partView.meshRenders.Count; i++)
                            {
                                partView.meshRenders[i].SetPropertyBlock(_materialPropertyBlock);
                            }
                        }
                    }
                }
            }
        }

        private static BoundsInt JoinBounds(BoundsInt a, BoundsInt b)
        {
            int xMin = Mathf.Min(a.min.x, b.min.x);
            int yMin = Mathf.Min(a.min.y, b.min.y);
            int zMin = Mathf.Min(a.min.z, b.min.z);
            int xMax = Mathf.Max(a.max.x, b.max.x);
            int yMax = Mathf.Max(a.max.y, b.max.y);
            int zMax = Mathf.Max(a.max.z, b.max.z);
            
            return new BoundsInt(xMin, yMin, zMin, xMax - xMin, yMax - yMin, zMax - zMin);
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