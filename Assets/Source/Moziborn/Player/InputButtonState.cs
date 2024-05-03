using System;

namespace Moziborn.Player
{
    [Flags]
    public enum InputButtonState
    {
        None = 0,
        IsHeldDown = 1,
        PressedThisFrame = 2,
        ReleasedThisFrame = 4,
    }
}