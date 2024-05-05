using Moziborn.Animation;
using UnityEngine;

namespace Moziborn.Buildings
{
    public class DoorPrefab : MonoBehaviour
    {
        public Animator animator;
        public Collider[] colliders;
        public AnimatorEventHandler animatorEventHandler;
        public ColliderTriggerHandler colliderTriggerHandler;
        public float switchDuration;
    }
}