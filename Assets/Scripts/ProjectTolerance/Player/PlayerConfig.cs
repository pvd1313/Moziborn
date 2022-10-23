using System;

namespace ProjectTolerance.Player
{
    [Serializable]
    public struct PlayerConfig
    {
        public PlayerView playerViewPrefab;
        public AimSquareView aimSquareViewPrefab;
        public float playerSpeed;
        public float aimSpeed;
    }
}