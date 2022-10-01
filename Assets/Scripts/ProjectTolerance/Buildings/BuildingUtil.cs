using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectTolerance.Buildings
{
    public static class BuildingUtil
    {
        public static void DetectBuildings(Tilemap floor0Frame, 
                                           Tilemap floor0Interactive, 
                                           Tilemap floor1Frame, 
                                           List<BuildingData> resultBuildings)
        {
            resultBuildings.Clear();
            
            var occupiedCells = new HashSet<Vector2Int>();
            GetTilemapOccupiedCells(floor1Frame, occupiedCells);

            var adjustedCells = new HashSet<Vector2Int>();
            var renderers = new List<MeshRenderer>();
            var rooms = new List<BoundsInt>();
            var doorFrames = new List<Vector2Int>();
            var walls = new List<Vector2Int>();
            var doors = new List<Vector2Int>();

            while (occupiedCells.Count > 0)
            {
                VisitAdjustedCells(occupiedCells.First(), occupiedCells, adjustedCells);
                GetBuildingParts(floor0Frame, adjustedCells, EBuildingPartType.DoorFrame, doorFrames);
                GetBuildingParts(floor0Frame, adjustedCells, EBuildingPartType.Wall, walls);
                GetBuildingParts(floor0Interactive, adjustedCells, EBuildingPartType.Door, doors);
                GetRoomBounds(adjustedCells, rooms);

                renderers.Clear();
                
                foreach (var room in rooms)
                {
                    for (var x = room.min.x; x <= room.max.x; x++)
                    for (var y = room.min.y; y <= room.max.y; y++)
                    {
                        if (TryGetRendererAtPosition(floor0Frame, x, y, out var meshRenderer))
                        {
                            renderers.Add(meshRenderer);
                        }
                        
                        if (TryGetRendererAtPosition(floor1Frame, x, y, out meshRenderer))
                        {
                            renderers.Add(meshRenderer);
                        }
                    }
                }

                resultBuildings.Add(new BuildingData
                {
                    rooms = rooms.ToArray(),
                    viewBlockingParts = renderers.ToArray(),
                    doorFrames = doorFrames.ToArray(),
                    walls = walls.ToArray(),
                    doors = doors.Select(pos => new DoorData
                    {
                        position = pos,
                        controller = floor0Interactive.GetInstantiatedObject(new Vector3Int(pos.x, pos.y, 0))
                            .GetComponentInChildren<DoorController>()
                    }).ToArray()
                });
            }

            bool TryGetRendererAtPosition(Tilemap tilemap, int x, int y, out MeshRenderer meshRenderer)
            {
                meshRenderer = null;

                var gameObject = tilemap.GetInstantiatedObject(new Vector3Int(x, y, 0));
                if (gameObject == null)
                {
                    return false;
                }

                var partData = gameObject.GetComponentInChildren<BuildingPartData>();
                if (partData.PartType == EBuildingPartType.Roof || 
                    partData.PartType == EBuildingPartType.Wall ||
                    partData.PartType == EBuildingPartType.DoorFrame)
                {
                    meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
                    return true;
                }

                return false;
            }
        }

        private static void GetBuildingParts(Tilemap tilemap, 
                                             HashSet<Vector2Int> cells, 
                                             EBuildingPartType partType,
                                             List<Vector2Int> resultParts)
        {
            resultParts.Clear();

            foreach (var cell in cells)
            {
                var gameObject = tilemap.GetInstantiatedObject(new Vector3Int(cell.x, cell.y, 0));
                if (gameObject == null)
                {
                    continue;
                }
                
                var partData = gameObject.GetComponentInChildren<BuildingPartData>();
                if (partData.PartType == partType)
                {
                    resultParts.Add(cell);
                }
            }
        }

        private static void GetRoomBounds(HashSet<Vector2Int> cells, List<BoundsInt> resultBounds)
        {
            resultBounds.Clear();
            
            while (cells.Count > 0)
            {
                var point = cells.First();
                var boundsMin = point;
                var boundsMax = point;

                var offset = 1;
                for (; cells.Contains(boundsMax + new Vector2Int(offset, 0)); offset++) ;
                boundsMax.x = boundsMax.x + offset - 1;

                offset = 1;
                for (; cells.Contains(boundsMin - new Vector2Int(offset, 0)); offset++) ;
                boundsMin.x = boundsMin.x - offset + 1;

                offset = 1;
                for (; TestRow(boundsMin.x, boundsMax.x, boundsMax.y + offset); offset++) ;
                boundsMax.y = boundsMax.y + offset - 1;
                
                offset = 1;
                for (; TestRow(boundsMin.x, boundsMax.x, boundsMin.y - offset); offset++) ;
                boundsMin.y = boundsMin.y - offset + 1;
                
                for (var x = boundsMin.x; x <= boundsMax.x; x++)
                    for (var y = boundsMin.y; y <= boundsMax.y; y++)
                        cells.Remove(new Vector2Int(x, y));

                var boundsDelta = boundsMax - boundsMin + Vector2Int.one;
                
                resultBounds.Add(new BoundsInt(boundsMin.x, boundsMin.y, 0, boundsDelta.x, boundsDelta.y, 1));
            }

            bool TestRow(int xMin, int xMax, int y)
            {
                for (var x = xMin; x <= xMax; x++)
                {
                    if (!cells.Contains(new Vector2Int(x, y)))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static void GetTilemapOccupiedCells(Tilemap tilemap, HashSet<Vector2Int> resultCells)
        {
            var cellBounds = tilemap.cellBounds;
            
            for (var x = cellBounds.min.x; x <= cellBounds.max.x; x++)
            for (var y = cellBounds.min.y; y <= cellBounds.max.y; y++)
            {
                if (tilemap.HasTile(new Vector3Int(x, y, 0)))
                {
                    resultCells.Add(new Vector2Int(x, y));
                } 
            }
        }

        private static void VisitAdjustedCells(Vector2Int startPoint, 
                                               HashSet<Vector2Int> availablePoints, 
                                               HashSet<Vector2Int> resultVisitedPoints)
        {
            resultVisitedPoints.Clear();
            var pointsStack = new Stack<Vector2Int>();
            
            pointsStack.Push(startPoint);

            while (pointsStack.Count > 0)
            {
                startPoint = pointsStack.Pop();
                if (!availablePoints.Remove(startPoint))
                {
                    continue;
                }
                
                resultVisitedPoints.Add(startPoint);
                
                pointsStack.Push(startPoint + Vector2Int.right);
                pointsStack.Push(startPoint + Vector2Int.left);
                pointsStack.Push(startPoint + Vector2Int.up);
                pointsStack.Push(startPoint + Vector2Int.down);
            }
        }
    }
}