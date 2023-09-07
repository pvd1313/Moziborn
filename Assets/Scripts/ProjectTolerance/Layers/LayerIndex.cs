using UnityEngine;

namespace ProjectTolerance.Layers
{
    public static class LayerIndex
    {
        public static readonly int Default = LayerMask.NameToLayer("Default");
        public static readonly int LightObstacle = LayerMask.NameToLayer("Light Obstacle");
        public static readonly int PointLight = LayerMask.NameToLayer("Torch Light");
        public static readonly int WarFogLight = LayerMask.NameToLayer("War Fog Light");
    }
}