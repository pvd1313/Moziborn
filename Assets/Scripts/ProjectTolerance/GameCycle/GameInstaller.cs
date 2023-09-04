using ProjectTolerance.Buildings;
using ProjectTolerance.Cinematics;
using ProjectTolerance.Coroutines;
using ProjectTolerance.Debugging;
using ProjectTolerance.Player;
using ProjectTolerance.Rendering.AmbientLight;
using ProjectTolerance.Rendering.TorchLight;
using ProjectTolerance.Rendering.Weather;
using UnityEngine;
using Zenject;

namespace ProjectTolerance.GameCycle
{
    public class GameInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private BuildingConfig _buildingConfig;
        [SerializeField] private AmbientLightConfig _ambientLightConfig;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private CameraConfig _cameraConfig;
        [SerializeField] private TorchLightConfig _torchLightConfig;
        [SerializeField] private WeatherConfig _weatherConfig;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameCycle>().AsSingle();
            Container.BindInterfacesAndSelfTo<CoroutineController>().AsSingle();
            Container.BindInterfacesAndSelfTo<GizmosController>().AsSingle();

            Container.BindInterfacesAndSelfTo<BuildingController>().AsSingle().WithArguments(_buildingConfig);
            Container.BindInterfacesAndSelfTo<AmbientLightController>().AsSingle().WithArguments(_ambientLightConfig);
            Container.BindInterfacesAndSelfTo<MainCameraController>().AsSingle().WithArguments(_cameraConfig);
            Container.BindInterfacesAndSelfTo<TorchLightController>().AsSingle().WithArguments(_torchLightConfig);
            Container.BindInterfacesAndSelfTo<WeatherController>().AsSingle().WithArguments(_weatherConfig);

            Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle().WithArguments(_playerConfig);
            Container.BindInterfacesAndSelfTo<PlayerInputController>().AsSingle();
        }
    }
}