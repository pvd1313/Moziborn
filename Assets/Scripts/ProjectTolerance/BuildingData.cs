using UnityEngine;

namespace ProjectTolerance
{
    public struct BuildingData
    {
        public Bounds[] rooms;
        public Bounds[] doorFrames;
        public MeshRenderer[] viewBlockingParts;
    }
}