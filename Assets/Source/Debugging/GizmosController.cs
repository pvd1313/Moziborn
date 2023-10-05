using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTolerance.Debugging
{
    public class GizmosController
    {
        private readonly List<IGizmosDrawer> _gizmosDrawers;

        public GizmosController (List<IGizmosDrawer> gizmosDrawers)
        {
            _gizmosDrawers = gizmosDrawers;
        }
        
        public IEnumerator Initialize()
        {
            GizmosBehaviour behaviour = new GameObject("GizmosController").AddComponent<GizmosBehaviour>();
            behaviour.onDrawGizmos += OnDrawGizmos;
            
            yield break;
        }

        private void OnDrawGizmos()
        {
            foreach (IGizmosDrawer entity in _gizmosDrawers)
            {
                entity.OnDrawGizmos();
            }
        }
    }
}