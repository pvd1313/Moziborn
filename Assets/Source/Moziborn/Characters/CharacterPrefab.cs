using Moziborn.Animation;
using UnityEngine;

namespace Moziborn.Characters
{
    public class CharacterPrefab : MonoBehaviour
    {
        public new Rigidbody rigidbody;
        public Collider viewBlockCollider;
        public Transform meshRoot;
        public Animator animator;
        public AnimatorEventHandler animatorEventHandler;
        public CapsuleCollider physicsCollider;
    }
}