using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Moziborn.GameCycle
{
    public class UpdatingBehaviour : MonoBehaviour
    {
        public event Action onUpdate;
        public event Action onLateUpdate;
        public event Action onFixedUpdate;

        private void Update()
        {
            onUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            onLateUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            onFixedUpdate?.Invoke();
        }
    }
}