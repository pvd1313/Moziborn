using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerView : MonoBehaviour
    {
        public Bounds colliderBounds => _collider.bounds;

        public Vector3 position
        {
            get => transform.position;
            set => transform.position = value;
        }
        
        [SerializeField] private Collider _collider;
    }
}