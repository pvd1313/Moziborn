using System;
using ProjectTolerance.Shading.Lighting;

namespace ProjectTolerance.Player
{
    [Serializable]
    public class PlayerConfig
    {
        public float meshSmoothTime;
        public PlayerView playerViewPrefab;
        public TorchLightView torchLightViewPrefab;
        public float torchLightAngle;
        public float torchLightDistance;
        public float playerSpeed;
    }
}