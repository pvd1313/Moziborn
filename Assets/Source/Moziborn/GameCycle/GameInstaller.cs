using System;
using System.Collections.Generic;
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
        [SerializeField] private List<ScriptableObject> _configs;

        public override void InstallBindings()
        {
            Type[] controllers = 
            {
                typeof(GameCycle),
                typeof(CoroutineController),
                typeof(GizmosController),
                
                typeof(BuildingController),
                typeof(AmbientLightController),
                typeof(MainCameraController),
                typeof(PointLightManager),
                typeof(PointLightRender),
                typeof(WeatherController),
                typeof(WarFogRender),
                
                typeof(PlayerController),
                typeof(PlayerInputController)
            };
            
            foreach (ScriptableObject config in _configs)
            {
                Container.BindInterfacesAndSelfTo(config.GetType()).FromInstance(config).AsSingle();
            }

            foreach (Type controller in controllers)
            {
                Container.BindInterfacesAndSelfTo(controller).AsSingle();
            }
        }
    }
}