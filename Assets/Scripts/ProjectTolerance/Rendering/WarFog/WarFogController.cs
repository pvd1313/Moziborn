using System.Collections;
using ProjectTolerance.Player;
using ProjectTolerance.Rendering.AmbientLight;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ProjectTolerance.Rendering.WarFog
{
    public class WarFogController
    {
        private readonly WarFogConfig _config;
        private readonly AmbientLightController _ambientLight;
        private readonly PlayerController _player;

        private Texture2D _sunTexture;
        private RenderTexture _stateTexture;
        private Vector2Int _playerPrevPosition;
        private Material _ambientMaterial;

        public WarFogController (WarFogConfig config, 
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
            
            for (int i = 0; i < wallTexture.width; i++)
                for (int j = 0; j < wallTexture.height; j++)
                    _sunTexture.SetPixel(i, j, Color.black);

            _sunTexture.Apply();

            _stateTexture = new RenderTexture(wallTexture.width, wallTexture.height, 0, wallTexture.graphicsFormat)
            {
                name = "WarFog_State",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear
            };

            _ambientMaterial = new Material(_config.ambientShader);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.WallTexture, wallTexture);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.SunTexture, _sunTexture);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.StateTexture, _stateTexture);

            Shader.SetGlobalTexture(ShaderPropertyId.WarFogMapTexture, _stateTexture);
            
            yield break;
        }

        public void Update()
        {
            BoundsInt textureBounds = _ambientLight.textureBounds;

            Vector3 offset = _player.position - new Vector3(textureBounds.position.x, 0, textureBounds.position.y);
            offset.x = Mathf.Clamp(offset.x, 0, textureBounds.size.x - 1);
            offset.y = Mathf.Clamp(offset.z, 0, textureBounds.size.y - 1);
            Vector2Int playerPosition = new Vector2Int(Mathf.FloorToInt(offset.x), Mathf.FloorToInt(offset.y));
            
            if (playerPosition != _playerPrevPosition)
            {
                _sunTexture.SetPixel(_playerPrevPosition.x, _playerPrevPosition.y, Color.black);
                _sunTexture.SetPixel(playerPosition.x, playerPosition.y, Color.white);
                _sunTexture.Apply();

                _playerPrevPosition = playerPosition;
            }
            
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.Blur, _config.blur);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.LinearFall, _config.linearFall);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.BilinearFall, _config.bilinearFall);
            
            RenderTexture tempTexture = RenderTexture.GetTemporary(
                textureBounds.size.x, 
                textureBounds.size.y, 
                0, 
                RenderTextureFormat.R16);
            
            Graphics.Blit(_stateTexture, tempTexture, _ambientMaterial);
            
            Graphics.Blit(tempTexture, _stateTexture);

            RenderTexture.ReleaseTemporary(tempTexture);
        }
    }
}