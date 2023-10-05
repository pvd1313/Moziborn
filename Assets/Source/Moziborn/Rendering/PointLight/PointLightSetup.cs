using UnityEngine;

namespace Moziborn.Rendering.PointLight
{
    public struct PointLightSetup
    {
        public float rotationRad;
        public Vector3 position;
        public float spreadAngleRad;
        public float range;
        public int layerIndex;
        public Material material;
    }
}