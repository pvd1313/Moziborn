using System;
using UnityEngine;

namespace ProjectTolerance.Debugging
{
    public class GizmosBehaviour : MonoBehaviour
    {
        public event Action onDrawGizmos;
        
        private void OnDrawGizmos()
        {
            onDrawGizmos?.Invoke();
        }
    }
}