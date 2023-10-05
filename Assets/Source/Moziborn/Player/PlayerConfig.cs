using UnityEngine;

namespace Moziborn.Player
{
    public class PlayerConfig : ScriptableObject
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