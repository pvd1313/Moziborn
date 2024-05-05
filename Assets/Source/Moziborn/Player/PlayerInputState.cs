using UnityEngine;

namespace Moziborn.Player
{
    public struct PlayerInputState
    {
        public Vector3 aimDirection;
        public Vector3 movementDirection;
        public PlayerInputMode mode;
        
        public InputButtonState removeAllFlaresButton;
        public InputButtonState dropFlareButton;
        public InputButtonState fireButton;
        public InputButtonState flashlightButton;
    }
}