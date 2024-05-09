using UnityEngine;

namespace Moziborn.AI
{
    public class MobControllerConfig : ScriptableObject
    {
        public NavigationSurfacePrefab navigationSurfacePrefab;
        public float mobRotationSpeed;
        public float agroDistance;
        public float agroDelay;
        public float aimAgroAngle;
    }
}