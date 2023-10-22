using System.Collections;
using Moziborn.Buildings;
using Moziborn.Player;
using UnityEngine;

namespace Moziborn.Rendering.WarFog
{
    public class WarFogRender
    {
        private readonly WarFogRenderConfig _config;
        private readonly BuildingController _buildingController;
        private readonly PlayerController _player;

        private Texture2D _sunTexture;
        private Vector2Int _playerPosition;
        private float _sampleTime;

        private WarFogSampler _groundSampler;
        private WarFogSampler _roofSampler;

        public WarFogRender (WarFogRenderConfig config, 
                             BuildingController buildingController, 
                             PlayerController player)
        {
            _config = config;
            _buildingController = buildingController;
            _player = player;
        }
        
        public IEnumerator Initialize()
        {
            _groundSampler = new WarFogSampler("Ground", _buildingController, _config);
            _roofSampler = new WarFogSampler("Roof", _buildingController, _config);
               
            BoundsInt levelBounds = _buildingController.bounds;
            
            _sunTexture = new Texture2D(levelBounds.size.x, levelBounds.size.y, TextureFormat.R16, false, true)
            {
                name = "WarFog_Sun",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            _sunTexture.Fill(Color.black);
            _sunTexture.Apply();

            Shader.SetGlobalTexture(ShaderPropertyID.Global.WarFogMapGroundTexture, _groundSampler.stateTextureWithPost);
            Shader.SetGlobalTexture(ShaderPropertyID.Global.WarFogMapRoofTexture, _roofSampler.stateTextureWithPost);
            
            yield break;
        }

        public void Update()
        {
            _sampleTime += Time.deltaTime;
            
            float sampleRate = 1 / _config.sampleRate;
            
            if (_sampleTime < sampleRate)
            {
                return;
            }
            
            BoundsInt levelBounds = _buildingController.bounds;

            _groundSampler._wallTexture.DrawDoors(_buildingController);
            _groundSampler._wallTexture.Apply();
            
            _roofSampler._wallTexture.Fill(Color.black);
            if (_buildingController.currentBuildingIndex != -1)
            {
                _roofSampler._wallTexture.DrawWalls(_buildingController, _buildingController.currentBuildingIndex, Color.white);
                _roofSampler._wallTexture.DrawDoors(_buildingController, _buildingController.currentBuildingIndex);
            }
            _roofSampler._wallTexture.Apply();

            Vector3 offset = _player.rigidbodyPosition - new Vector3(levelBounds.position.x, 0, levelBounds.position.y);
            float u = Mathf.Clamp01(offset.x / levelBounds.size.x);
            float v = Mathf.Clamp01(offset.z / levelBounds.size.y);
            Vector2Int playerPosNew = new (Mathf.FloorToInt(u * _sunTexture.width), Mathf.FloorToInt(v * _sunTexture.height));
            
            if (_playerPosition != playerPosNew)
            {
                _sunTexture.SetPixel(_playerPosition.x, _playerPosition.y, Color.black);
                _sunTexture.SetPixel(playerPosNew.x, playerPosNew.y, Color.white);
                _sunTexture.Apply();

                _playerPosition = playerPosNew;
            }

            while (_sampleTime >= sampleRate)
            {
                _groundSampler.Sample(_sunTexture);
                _roofSampler.Sample(_sunTexture);
                
                _sampleTime -= sampleRate;
            }
        }
    }
}