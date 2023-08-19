using System;

namespace ProjectTolerance.Player
{
    [Serializable]
    public class PlayerConfig
    {
        public float meshSmoothTime;
        public PlayerView playerViewPrefab;
        public float torchLightAngle;
        public float torchLightDistance;
        public float playerSpeed;
    }
}