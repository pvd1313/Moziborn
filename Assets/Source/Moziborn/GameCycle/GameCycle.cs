using System.Collections;
using Moziborn.Audio;
using Moziborn.Buildings;
using Moziborn.Characters;
using Moziborn.Cinematics;
using Moziborn.Coroutines;
using Moziborn.Debugging;
using Moziborn.Player;
using Moziborn.Rendering.AmbientLight;
using Moziborn.Rendering.PointLight;
using Moziborn.Rendering.WarFog;
using Moziborn.Rendering.Weather;
using UnityEngine;
using Zenject;

namespace Moziborn.GameCycle
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
            yield return _container.TryResolve<AudioController>()?.Initialize();
            yield return _container.TryResolve<CharacterFactory>()?.Initialize();
            yield return _container.TryResolve<BuildingController>()?.Initialize();
            yield return _container.TryResolve<MainCameraController>()?.Initialize();
            yield return _container.TryResolve<PointLightManager>()?.Initialize();
            yield return _container.TryResolve<PointLightRender>()?.Initialize();
            yield return _container.TryResolve<AmbientLightRender>()?.Initialize();
            yield return _container.TryResolve<PlayerController>()?.Initialize();
            yield return _container.TryResolve<WeatherController>()?.Initialize();
            yield return _container.TryResolve<WarFogRender>()?.Initialize();
            yield return _container.TryResolve<GizmosController>()?.Initialize();
            
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
            _container.TryResolve<PointLightManager>()?.Update();
            _container.TryResolve<AmbientLightRender>()?.Update();
            _container.TryResolve<WarFogRender>()?.Update();
            _container.TryResolve<BuildingController>()?.Update(false);
            _container.TryResolve<AudioController>()?.Update();
        }
    }
}