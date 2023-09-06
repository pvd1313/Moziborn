using UnityEngine;

namespace ProjectTolerance.Layers
{
    public static class LayerId
    {
        public static readonly int Default = LayerMask.NameToLayer("Default");
        public static readonly int ViewObstacle = LayerMask.NameToLayer("View Obstacle");
        public static readonly int TorchLight = LayerMask.NameToLayer("Torch Light");
        public static readonly int WarFog = LayerMask.NameToLayer("War Fog");
    }
}