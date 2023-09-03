using UnityEngine;

namespace ProjectTolerance.Player
{
    public struct PlayerInputState
    {
        public Vector2 aimDirection;
        public Vector2 movementDirection;
        public PlayerInputMode mode;

        public bool isDropFlareKeyPressed;
        public bool isRemoveAllFlaresKeyPressed;
    }
}