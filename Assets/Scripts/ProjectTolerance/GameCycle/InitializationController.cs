using System.Collections;
using ProjectTolerance.Buildings;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Coroutines;
using ProjectTolerance.Debugging;
using ProjectTolerance.Player;
using ProjectTolerance.Shading.Lighting;
using Zenject;

namespace ProjectTolerance.GameCycle
{
    public class InitializationController : IInitializable
    {
        private readonly DiContainer _container;
        private readonly CoroutineController _coroutineController;

        public InitializationController (DiContainer container, CoroutineController coroutineController)
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
            
            yield return _container.TryResolve<UpdatingController>()?.Initialize();
        }
    }
}