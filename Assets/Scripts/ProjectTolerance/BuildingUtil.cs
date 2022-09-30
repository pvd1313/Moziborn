using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectTolerance
{
    public static class BuildingUtil
    {
        public static void DetectBuildings(Tilemap floor0, Tilemap floor1, List<BuildingData> resultBuildings)
        {
            resultBuildings.Clear();
            
            var occupiedCells = new HashSet<Vector2Int>();
            GetTilemapOccupiedCells(floor1, occupiedCells);

            var adjustedCells = new HashSet<Vector2Int>();
            var renderers = new List<MeshRenderer>();
            var rooms = new List<BoundsInt>();

            while (occupiedCells.Count > 0)
            {
                VisitAdjustedCells(occupiedCells.First(), occupiedCells, adjustedCells);
                GetRoomBounds(adjustedCells, rooms);

                renderers.Clear();
                
                foreach (var room in rooms)
                {
                    for (var x = room.min.x; x <= room.max.x; x++)
                    for (var y = room.min.y; y <= room.max.y; y++)
                    {
                        if (TryGetRendererAtPosition(floor0, x, y, out var meshRenderer))
                        {
                            renderers.Add(meshRenderer);
                        }
                        
                        if (TryGetRendererAtPosition(floor1, x, y, out meshRenderer))
                        {
                            renderers.Add(meshRenderer);
                        }
                    }
                }

                resultBuildings.Add(new BuildingData
                {
                    rooms = rooms.Select(r => new Bounds(new Vector3(r.center.x, 1.65f, r.center.y), 
                        new Vector3(r.size.x, 3.3f, r.size.y))).ToArray(),
                    viewBlockingParts = renderers.ToArray()
                });
            }

            bool TryGetRendererAtPosition(Tilemap tilemap, int x, int y, out MeshRenderer meshRenderer)
            {
                meshRenderer = null;

                var gameObject = tilemap.GetInstantiatedObject(new Vector3Int(x, y, 0));
                if (gameObject == null)
                {
                    Debug.Assert(!tilemap.HasTile(new Vector3Int(x, y, 0)), "Tilemap has tile, but gameObject is not spawned yet");
                    
                    return false;
                }

                var partData = gameObject.GetComponent<BuildingPartData>();
                if (partData.PartType == EBuildingPartType.Roof || partData.PartType == EBuildingPartType.Wall)
                {
                    meshRenderer = gameObject.GetComponent<MeshRenderer>();
                    return true;
                }

                return false;
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
                {
                    cells.Remove(new Vector2Int(x, y));
                }

                var boundsDelta = boundsMax - boundsMin + Vector2Int.one;
                
                resultBounds.Add(new BoundsInt(boundsMin.x, boundsMin.y, 0, boundsDelta.x, boundsDelta.y, 0));
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