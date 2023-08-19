using UnityEngine;

namespace ProjectTolerance.Layers
{
    public static class LayerId
    {
        public static int Default = LayerMask.NameToLayer("Default");
        public static int ViewObstacle = LayerMask.NameToLayer("View Obstacle");
    }
}