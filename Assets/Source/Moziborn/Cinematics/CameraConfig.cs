using System;
using UnityEngine;

namespace Moziborn.Cinematics
{
    [Serializable]
    public class CameraConfig
    {
        public Vector3 rotationAngleDegrees;
        public float orthographicSize;
        public CameraView viewPrefab;
        public float smoothTime;
    }
}