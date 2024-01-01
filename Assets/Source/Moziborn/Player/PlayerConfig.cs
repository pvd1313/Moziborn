using Moziborn.Characters;
using UnityEngine;
using UnityEngine.Serialization;

namespace Moziborn.Player
{
    public class PlayerConfig : ScriptableObject
    {
        [FormerlySerializedAs("playerPrefab")] public CharacterMeshPrefab playerMeshPrefab;
        public float positionSmoothTime;
        public float rotationSmoothTime;
        public float flashlightAngleDeg;
        public float flashlightRange;
        public float viewRange;
        public float playerSpeed;
    }
}