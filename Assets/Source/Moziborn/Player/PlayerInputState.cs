using UnityEngine;

namespace Moziborn.Player
{
    public struct PlayerInputState
    {
        public Vector3 aimDirection;
        public Vector3 movementDirection;
        public PlayerInputMode mode;

        public bool isDropFlareKeyPressed;
        public bool isRemoveAllFlaresKeyPressed;
    }
}