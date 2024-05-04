using UnityEngine;

namespace Moziborn.Animation
{
    public static class AnimatorPropertyId
    {
        public static int walkSpeed { get; } = Animator.StringToHash("WalkSpeed");
        public static int isOpen { get; } = Animator.StringToHash("IsOpen");
    }
}