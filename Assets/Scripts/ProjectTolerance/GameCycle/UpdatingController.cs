using System.Collections;
using System.Collections.Generic;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Player;
using ProjectTolerance.Shading.Lighting;
using UnityEngine;
using Zenject;

namespace ProjectTolerance.GameCycle
{
    public class UpdatingController
    {
        private readonly List<IUnorderedUpdatable> _updatables;
        private readonly List<IUnorderedFixedUpdatable> _fixedUpdatables;
        private readonly List<IUnorderedLateUpdatable> _lateUpdatables;
        private readonly DiContainer _diContainer;

        public UpdatingController (List<IUnorderedUpdatable> updatables, 
                                   List<IUnorderedFixedUpdatable> fixedUpdatables,
                                   List<IUnorderedLateUpdatable> lateUpdatables,
                                   DiContainer diContainer)
        {
            _updatables = updatables;
            _fixedUpdatables = fixedUpdatables;
            _lateUpdatables = lateUpdatables;
            _diContainer = diContainer;
        }
        
        public IEnumerator Initialize()
        {
            UpdatingBehaviour behaviour = new GameObject("UpdatingController").AddComponent<UpdatingBehaviour>();

            behaviour.onUpdate += Update;
            behaviour.onLateUpdate += LateUpdate;
            behaviour.onFixedUpdate += FixedUpdate;
            
            yield break;
        }

        private void Update()
        {
            foreach (IUnorderedUpdatable entity in _updatables)
            {
                entity.Update();
            }
            
            _diContainer.TryResolve<PlayerController>()?.Update();
            _diContainer.TryResolve<MainCameraController>()?.Update();
            _diContainer.TryResolve<TorchLightController>()?.Update();
        }

        private void LateUpdate()
        {
            foreach (IUnorderedLateUpdatable entity in _lateUpdatables)
            {
                entity.LateUpdate();
            }
        }

        private void FixedUpdate()
        {
            foreach (IUnorderedFixedUpdatable entity in _fixedUpdatables)
            {
                entity.FixedUpdate();
            }
        }
    }
}