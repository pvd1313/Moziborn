using System;
using UnityEngine;

namespace Moziborn.Animation
{
    public class AnimatorEventHandler : MonoBehaviour
    {
        public event Action<string> animatorEvent; 
        
        public void OnFootStep()
        {
            animatorEvent?.Invoke(AnimatorEventId.footStep);
        }
    }
}