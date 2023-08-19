using System;

namespace ProjectTolerance.Player
{
    [Serializable]
    public class PlayerConfig
    {
        public float meshSmoothTime;
        public PlayerView playerViewPrefab;
        public float torchLightAngleDeg;
        public float torchLightDistance;
        public float playerSpeed;
    }
}