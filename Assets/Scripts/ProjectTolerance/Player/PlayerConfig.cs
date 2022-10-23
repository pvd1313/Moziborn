using System;

namespace ProjectTolerance.Player
{
    [Serializable]
    public struct PlayerConfig
    {
        public PlayerView playerViewPrefab;
        public float movementSpeed;
    }
}