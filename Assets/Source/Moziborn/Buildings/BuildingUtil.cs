using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Moziborn.Buildings
{
    public static class BuildingUtil
    {
        public static void DetectBuildings(Tilemap floor0Frame, 
                                           Tilemap floor0Interactive, 
                                           Tilemap floor1Frame, 
                                           List<BuildingData> resultBuildings)
        {
            resultBuildings.Clear();
            
            HashSet<Vector2Int> occupiedCells = new ();
            GetTilemapOccupiedCells(floor1Frame, occupiedCells);

            HashSet<Vector2Int> adjustedCells = new ();
            List<MeshRenderer> renderers = new ();
            List<BoundsInt> rooms = new ();
            List<Vector2Int> doorFrames = new ();
            List<Vector2Int> walls = new ();
            List<Vector2Int> doors = new ();

            while (occupiedCells.Count > 0)
            {
                VisitAdjustedCells(occupiedCells.First(), occupiedCells, adjustedCells);
                GetBuildingParts(floor0Frame, adjustedCells, BuildingPartType.DoorFrame, doorFrames);
                GetBuildingParts(floor0Frame, adjustedCells, BuildingPartType.Wall, walls);
                GetBuildingParts(floor0Interactive, adjustedCells, BuildingPartType.Door, doors);
                GetRoomBounds(adjustedCells, rooms);

                renderers.Clear();
                
                foreach (BoundsInt room in rooms)
                {
                    for (int x = room.min.x; x <= room.max.x; x++)
                    for (int y = room.min.y; y <= room.max.y; y++)
                    {
                        if (TryGetRendererAtPosition(floor0Frame, x, y, out MeshRenderer meshRenderer))
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
                        view = floor0Interactive.GetInstantiatedObject(new Vector3Int(pos.x, pos.y, 0)).GetComponentInChildren<DoorView>()
                    }).ToArray()
                });
            }

            bool TryGetRendererAtPosition(Tilemap tilemap, int x, int y, out MeshRenderer meshRenderer)
            {
                meshRenderer = null;

                GameObject gameObject = tilemap.GetInstantiatedObject(new Vector3Int(x, y, 0));
                if (gameObject == null)
                {
                    return false;
                }

                BuildingPartView partData = gameObject.GetComponentInChildren<BuildingPartView>();
                if (partData.partType is BuildingPartType.Roof or BuildingPartType.Wall or BuildingPartType.DoorFrame)
                {
                    meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
                    return true;
                }

                return false;
            }
        }

        private static void GetBuildingParts(Tilemap tilemap, 
                                             HashSet<Vector2Int> cells, 
                                             BuildingPartType partType,
                                             List<Vector2Int> resultParts)
        {
            resultParts.Clear();

            foreach (Vector2Int cell in cells)
            {
                GameObject gameObject = tilemap.GetInstantiatedObject(new Vector3Int(cell.x, cell.y, 0));
                if (gameObject == null)
                {
                    continue;
                }
                
                BuildingPartView partData = gameObject.GetComponentInChildren<BuildingPartView>();
                if (partData.partType == partType)
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
                Vector2Int point = cells.First();
                Vector2Int boundsMin = point;
                Vector2Int boundsMax = point;

                int offset = 1;
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
                
                for (int x = boundsMin.x; x <= boundsMax.x; x++)
                    for (int y = boundsMin.y; y <= boundsMax.y; y++)
                        cells.Remove(new Vector2Int(x, y));

                Vector2Int boundsDelta = boundsMax - boundsMin + Vector2Int.one;
                
                resultBounds.Add(new BoundsInt(boundsMin.x, boundsMin.y, 0, boundsDelta.x, boundsDelta.y, 1));
            }

            bool TestRow(int xMin, int xMax, int y)
            {
                for (int x = xMin; x <= xMax; x++)
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
            BoundsInt cellBounds = tilemap.cellBounds;
            
            for (int x = cellBounds.min.x; x <= cellBounds.max.x; x++)
            for (int y = cellBounds.min.y; y <= cellBounds.max.y; y++)
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
            Stack<Vector2Int> pointsStack = new ();
            
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