using UnityEngine;

namespace Moziborn.Cinematics
{
    public class CameraConfig : ScriptableObject
    {
        public Vector3 rotationAngleDegrees;
        public float orthographicSize;
        public CameraView viewPrefab;
        public float smoothTime;
    }
}