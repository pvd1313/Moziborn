using Moziborn.Characters;
using UnityEngine;

namespace Moziborn.Player
{
    public class PlayerConfig : ScriptableObject
    {
        public CharacterMeshPrefab playerMeshPrefab;
        public float positionSmoothTime;
        public float rotationSmoothTime;
        public float rotationSpeed;
        public float flashlightAngleDeg;
        public float flashlightRange;
        public float viewRange;
        public float movementSpeed;
    }
}