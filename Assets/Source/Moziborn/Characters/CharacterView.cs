using UnityEngine;

namespace Moziborn.Characters
{
    public class CharacterView : MonoBehaviour
    {
        public new Rigidbody rigidbody;
        public Collider viewBlockCollider;
        public Transform meshRoot;
        public Animator animator;
        public CapsuleCollider physicsCollider;
    }
}