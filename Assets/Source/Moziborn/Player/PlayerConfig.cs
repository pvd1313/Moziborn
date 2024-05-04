using Moziborn.Characters;
using Moziborn.VisualEffects;
using UnityEngine;

namespace Moziborn.Player
{
    public class PlayerConfig : ScriptableObject
    {
        public CharacterMeshPrefab playerMeshPrefab;
        public AimLinePrefab aimLineViewPrefab;
        
        public float positionSmoothTime;
        public float rotationSmoothTime;
        
        public float movementSpeed;
        public float rotationSpeed;
       
        public float flashlightAngleDeg;
        public float flashlightRange;

        public float gunChargeDelay;
        public float aimLineFadeDelay;
    }
}