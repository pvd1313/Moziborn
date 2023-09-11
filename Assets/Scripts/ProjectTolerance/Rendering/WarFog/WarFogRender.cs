using System.Collections;
using ProjectTolerance.Player;
using ProjectTolerance.Rendering.AmbientLight;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ProjectTolerance.Rendering.WarFog
{
    public class WarFogRender
    {
        private readonly WarFogRenderConfig _config;
        private readonly AmbientLightController _ambientLight;
        private readonly PlayerController _player;

        private Texture2D _sunTexture;
        private RenderTexture _stateTextureSpread;
        private RenderTexture _stateTextureSpreadAndBlur;
        private Vector2Int _playerPrevPosition;
        private Material _ambientMaterial;

        public WarFogRender (WarFogRenderConfig config, 
                             AmbientLightController ambientLight, 
                             PlayerController player)
        {
            _config = config;
            _ambientLight = ambientLight;
            _player = player;
        }
        
        public IEnumerator Initialize()
        {
            Texture2D wallTexture = _ambientLight.wallTexture;

            _sunTexture = new Texture2D(wallTexture.width, wallTexture.height, wallTexture.graphicsFormat, TextureCreationFlags.None)
            {
                name = "WarFog_Sun",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            
            for (int i = 0; i < _sunTexture.width; i++)
            for (int j = 0; j < _sunTexture.height; j++)
                _sunTexture.SetPixel(i, j, Color.black);

            _sunTexture.Apply();

            _stateTextureSpread = new RenderTexture(wallTexture.width, wallTexture.height, 0, wallTexture.graphicsFormat)
            {
                name = "WarFogStateTextureRaw",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            _ambientMaterial = new Material(_config.ambientShader);

            Shader.SetGlobalTexture(ShaderPropertyId.WarFogMapTexture, _stateTextureSpread);
            
            yield break;
        }

        public void Update()
        {
            BoundsInt textureBounds = _ambientLight.textureBounds;

            Vector3 offset = _player.rigidbodyPosition - new Vector3(textureBounds.position.x, 0, textureBounds.position.y);
            float u = Mathf.Clamp01(offset.x / _ambientLight.textureBounds.size.x);
            float v = Mathf.Clamp01(offset.z / _ambientLight.textureBounds.size.y);
            Vector2Int playerPosition = new (Mathf.FloorToInt(u * _sunTexture.width), Mathf.FloorToInt(v * _sunTexture.height));
            
            if (playerPosition != _playerPrevPosition)
            {
                _sunTexture.SetPixel(_playerPrevPosition.x, _playerPrevPosition.y, Color.black);
                _sunTexture.SetPixel(playerPosition.x, playerPosition.y, Color.white);
                _sunTexture.Apply();

                _playerPrevPosition = playerPosition;
            }
            
            RenderTexture tempTexture = RenderTexture.GetTemporary(
                textureBounds.size.x, 
                textureBounds.size.y, 
                0, 
                RenderTextureFormat.R16);
            
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.WallTexture, _ambientLight.wallTexture);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.SunTexture, _sunTexture);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.StateTexture, _stateTextureSpread);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.Blur, _config.blur);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.LinearFall, _config.linearFall);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.BilinearFall, _config.bilinearFall);

            Graphics.Blit(null, tempTexture, _ambientMaterial);
            Graphics.Blit(tempTexture, _stateTextureSpread);

            RenderTexture.ReleaseTemporary(tempTexture);
        }
    }
}