using UnityEngine;

namespace Moziborn.Buildings
{
    public struct BuildingData
    {
        public BoundsInt[] rooms;
        public Vector2Int[] doorFrames;
        public Vector2Int[] walls;
        public DoorData[] doors;

        public BuildingPartPrefab[] allParts;
    }
}