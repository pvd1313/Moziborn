using UnityEngine;

namespace ProjectTolerance.Mathematics
{
    public static class UnityVectorExtensions
    {
        public static Vector2 XZ(this Vector3 source)
        {
            return new Vector2(source.x, source.z);
        }
    }
}