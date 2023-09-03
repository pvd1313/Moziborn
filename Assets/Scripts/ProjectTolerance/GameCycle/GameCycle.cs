using System.Collections;
using ProjectTolerance.Buildings;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Coroutines;
using ProjectTolerance.Debugging;
using ProjectTolerance.Player;
using ProjectTolerance.Shading.Lighting;
using UnityEngine;
using Zenject;

namespace ProjectTolerance.GameCycle
{
    public class GameCycle : IInitializable
    {
        private readonly DiContainer _container;
        private readonly CoroutineController _coroutineController;

        public GameCycle (DiContainer container, CoroutineController coroutineController)
        {
            _container = container;
            _coroutineController = coroutineController;
        }
        
        public void Initialize()
        {
            _coroutineController.Initialize();

            _coroutineController.StartCoroutine(InitializationRoutine());
        }

        private IEnumerator InitializationRoutine()
        {
            yield return _container.TryResolve<GizmosController>()?.Initialize();
            yield return _container.TryResolve<BuildingController>()?.Initialize();
            yield return _container.TryResolve<TorchLightController>()?.Initialize();
            yield return _container.TryResolve<AmbientLightController>()?.Initialize();
            yield return _container.TryResolve<MainCameraController>()?.Initialize();
            yield return _container.TryResolve<PlayerController>()?.Initialize();
            yield return _container.TryResolve<WeatherController>()?.Initialize();
            
            UpdatingBehaviour behaviour = new GameObject("GameCycle").AddComponent<UpdatingBehaviour>();
            behaviour.onUpdate += Update;
            behaviour.onFixedUpdate += FixedUpdate;
        }
        
        private void FixedUpdate()
        {
            _container.TryResolve<PlayerController>()?.FixedUpdate();
        }

        private void Update()
        {
            _container.TryResolve<PlayerInputController>()?.Update();
            _container.TryResolve<PlayerController>()?.Update();
            _container.TryResolve<MainCameraController>()?.Update();
            _container.TryResolve<TorchLightController>()?.Update();
            _container.TryResolve<AmbientLightController>()?.Update();
            _container.TryResolve<BuildingController>()?.Update();
        }
    }
}