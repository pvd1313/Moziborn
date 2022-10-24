using UnityEngine;

namespace ProjectTolerance.Player
{
    public class AimSquareView : MonoBehaviour
    {
        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }
    }
}