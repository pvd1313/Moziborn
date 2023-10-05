using System;
using UnityEngine;

namespace Moziborn.Debugging
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