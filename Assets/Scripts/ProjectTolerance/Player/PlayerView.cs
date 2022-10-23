using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerView : MonoBehaviour
    {
        public Bounds ColliderBounds => _collider.bounds;

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        [SerializeField] private Collider _collider;
    }
}