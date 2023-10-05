using Moziborn.Buildings;
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
    public class GameInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private BuildingConfig _buildingConfig;
        [SerializeField] private AmbientLightConfig _ambientLightConfig;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private CameraConfig _cameraConfig;
        [SerializeField] private PointLightManagerConfig _pointLightManagerConfig;
        [SerializeField] private PointLightRenderConfig _pointLightRenderConfig;
        [SerializeField] private WeatherConfig _weatherConfig;
        [SerializeField] private WarFogRenderConfig _warFogRenderConfig;

        public override void InstallBindings()
        {
            Bind<GameCycle>();
            Bind<CoroutineController>();
            Bind<GizmosController>();

            Bind<BuildingController>(_buildingConfig);
            Bind<AmbientLightController>(_ambientLightConfig);
            Bind<MainCameraController>(_cameraConfig);
            Bind<PointLightManager>(_pointLightManagerConfig);
            Bind<PointLightRender>(_pointLightRenderConfig);
            Bind<WeatherController>(_weatherConfig);
            Bind<WarFogRender>(_warFogRenderConfig);

            Bind<PlayerController>(_playerConfig);
            Bind<PlayerInputController>();
        }

        private void Bind<T>()
        {
            Container.BindInterfacesAndSelfTo<T>().AsSingle();
        }

        private void Bind<T>(object config)
        {
            Container.BindInterfacesAndSelfTo<T>().AsSingle().WithArguments(config);
        }
    }
}