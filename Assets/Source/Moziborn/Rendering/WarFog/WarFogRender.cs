using System.Collections;
using Moziborn.Player;
using Moziborn.Rendering.AmbientLight;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Moziborn.Rendering.WarFog
{
    public class WarFogRender
    {
        private readonly WarFogRenderConfig _config;
        private readonly AmbientLightController _ambientLight;
        private readonly PlayerController _player;

        private Texture2D _sunTexture;
        private RenderTexture _stateTextureSpread;
        private RenderTexture _stateTextureSpreadAndPost;
        private Vector2Int _playerPrevPosition;
        private Material _ambientMaterial;
        private Material _postMergeMaterial;
        private Material _postFadeMaterial;
        private float _sampleTime;

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

            _stateTextureSpread = new RenderTexture(wallTexture.width, wallTexture.height, 0, RenderTextureFormat.R16)
            {
                name = "WarFogStateTextureRaw",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            _stateTextureSpreadAndPost = new RenderTexture(wallTexture.width * 2, wallTexture.height * 2, 0, RenderTextureFormat.R16)
            {
                name = "WarFogStateTexturePost",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };

            _ambientMaterial = new Material(_config.ambientShader);
            _postMergeMaterial = new Material(_config.postMergeShader);
            _postFadeMaterial = new Material(_config.postFadeShader);

            Shader.SetGlobalTexture(ShaderPropertyId.WarFogMapTexture, _stateTextureSpreadAndPost);
            
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

            _sampleTime += Time.deltaTime;

            float sampleRate = 1 / _config.sampleRate;

            while (_sampleTime >= sampleRate)
            {
                SampleTexture(textureBounds);
                
                _sampleTime -= sampleRate;
            }
        }

        private void SampleTexture(BoundsInt textureBounds)
        {
            RenderTexture tempTexture = RenderTexture.GetTemporary(
                textureBounds.size.x, 
                textureBounds.size.y, 
                0, 
                RenderTextureFormat.R16);
            
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.WallTexture, _ambientLight.wallTexture);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.SunTexture, _sunTexture);
            _ambientMaterial.SetTexture(ShaderPropertyId.Ambient.StateTexture, _stateTextureSpread);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.Blur, _config.ambientBlur);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.LinearFall, _config.ambientLinearFall);
            _ambientMaterial.SetFloat(ShaderPropertyId.Ambient.BilinearFall, _config.ambientBilinearFall);

            Graphics.Blit(null, tempTexture, _ambientMaterial);
            Graphics.Blit(tempTexture, _stateTextureSpread);
            RenderTexture.ReleaseTemporary(tempTexture);
            
            RenderTexture mergedTexture = RenderTexture.GetTemporary(
                textureBounds.size.x * 2, 
                textureBounds.size.y * 2, 
                0, 
                RenderTextureFormat.R16);
            
            Graphics.Blit(_stateTextureSpread, mergedTexture, _postMergeMaterial);

            RenderTexture fadedTexture = RenderTexture.GetTemporary(
                textureBounds.size.x * 2,
                textureBounds.size.y * 2,
                0,
                RenderTextureFormat.R16);
            
            _postFadeMaterial.SetTexture(ShaderPropertyId.WarFogFadePost.StateTexture, _stateTextureSpreadAndPost);
            _postFadeMaterial.SetTexture(ShaderPropertyId.WarFogFadePost.SunTexture, mergedTexture);
            _postFadeMaterial.SetFloat(ShaderPropertyId.WarFogFadePost.FadeDelta, _config.fadeDelta);
            
            Graphics.Blit(null, fadedTexture, _postFadeMaterial);
            Graphics.Blit(fadedTexture, _stateTextureSpreadAndPost);
            
            RenderTexture.ReleaseTemporary(mergedTexture);
            RenderTexture.ReleaseTemporary(fadedTexture);
        }
    }
}