using System.Collections;
using UnityEngine;

namespace Moziborn.Coroutines
{
    public class CoroutineController
    {
        private MonoBehaviour _behaviour;
        
        public void Initialize()
        {
            _behaviour = new GameObject("CoroutineController").AddComponent<CoroutineBehaviour>();
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            return _behaviour.StartCoroutine(routine);
        }

        public void StopCoroutine(Coroutine routine)
        {
            _behaviour.StopCoroutine(routine);
        }
    }
}