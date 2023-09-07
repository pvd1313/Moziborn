using UnityEngine;

namespace ProjectTolerance.Rendering.TorchLight
{
    public struct TorchLightSetup
    {
        public float rotationRad;
        public Vector3 position;
        public float spreadAngleRad;
        public float range;
        public int layerIndex;
        public Material material;
    }
}