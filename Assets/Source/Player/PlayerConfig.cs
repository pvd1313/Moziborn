using System;

namespace ProjectTolerance.Player
{
    [Serializable]
    public class PlayerConfig
    {
        public PlayerView playerViewPrefab;
        public float positionSmoothTime;
        public float rotationSmoothTime;
        public float flashlightAngleDeg;
        public float flashlightRange;
        public float viewRange;
        public float playerSpeed;
    }
}