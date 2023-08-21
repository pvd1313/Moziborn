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
    public class GameInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private BuildingConfig _buildingConfig;
        [SerializeField] private AmbientLightConfig _ambientLightConfig;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private CameraConfig _cameraConfig;
        [SerializeField] private TorchLightConfig _torchLightConfig;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InitializationController>().AsSingle();
            Container.BindInterfacesAndSelfTo<UpdatingController>().AsSingle();
            Container.BindInterfacesAndSelfTo<CoroutineController>().AsSingle();
            Container.BindInterfacesAndSelfTo<GizmosController>().AsSingle();

            Container.BindInterfacesAndSelfTo<BuildingController>().AsSingle().WithArguments(_buildingConfig);
            Container.BindInterfacesAndSelfTo<AmbientLightController>().AsSingle().WithArguments(_ambientLightConfig);
            Container.BindInterfacesAndSelfTo<MainCameraController>().AsSingle().WithArguments(_cameraConfig);
            Container.BindInterfacesAndSelfTo<TorchLightController>().AsSingle().WithArguments(_torchLightConfig);

            Container.BindInterfacesAndSelfTo<PlayerController>().AsSingle().WithArguments(_playerConfig);
            Container.BindInterfacesAndSelfTo<PlayerInputController>().AsSingle();
        }
    }
}