using System.Collections.Generic;
using UnityEngine;

namespace Moziborn.Buildings
{
    public class ColliderTriggerHandler : MonoBehaviour
    {
        private List<Collider> _activeTriggers;
        
        private void Awake()
        {
            _activeTriggers = new List<Collider>();
        }

        public bool CheckTriggerActive()
        {
            for (int i = _activeTriggers.Count - 1; i >= 0; i--)
            {
                if (_activeTriggers[i] == null)
                {
                    _activeTriggers.RemoveAt(i);
                }    
            }

            return _activeTriggers.Count > 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            _activeTriggers.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            _activeTriggers.Remove(other);
        }
    }
}