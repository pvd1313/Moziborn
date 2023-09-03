using System;

namespace ProjectTolerance.Player
{
    [Serializable]
    public class PlayerConfig
    {
        public PlayerView playerViewPrefab;
        public float positionSmoothTime;
        public float rotationSmoothTime;
        public float torchLightAngleDeg;
        public float torchLightDistance;
        public float playerSpeed;
    }
}