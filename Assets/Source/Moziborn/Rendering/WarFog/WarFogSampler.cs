using System;
using Moziborn.Buildings;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Moziborn.Rendering.WarFog
{
    public class WarFogSampler : IDisposable
    {
        public Texture2D _wallTexture { get; }
        public RenderTexture stateTextureWithPost { get; }
        
        private readonly WarFogRenderConfig _config;
        private readonly ILevelLayout _levelLayout;
        private readonly RenderTexture _stateTexture;
        private readonly Material _ambientMaterial;
        private readonly Material _postMergeMaterial;
        private readonly Material _postFadeMaterial;

        private bool _disposed;

        public WarFogSampler(string samplerName, ILevelLayout levelLayout, WarFogRenderConfig config)
        {
            _config = config;
            
            BoundsInt levelBounds = levelLayout.bounds;
            
            _wallTexture = new Texture2D(levelBounds.size.x, levelBounds.size.y, TextureFormat.R16, false, true)
            {
                name = "WarFog_" + samplerName + "_Wall",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            _stateTexture = new RenderTexture(levelBounds.size.x, levelBounds.size.y, 0, RenderTextureFormat.R16)
            {
                name = "WarFog_" + samplerName + "_StateTexture",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            stateTextureWithPost = new RenderTexture(levelBounds.size.x * 2, levelBounds.size.y* 2, 0, RenderTextureFormat.R16)
            {
                name = "WarFog_" + samplerName + "_StateTextureWithPost",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            
            _wallTexture.Fill(Color.black);
            _wallTexture.DrawWalls(levelLayout, Color.white);
            _wallTexture.Apply();
            
            _ambientMaterial = new Material(_config.ambientShader);
            _postMergeMaterial = new Material(_config.postMergeShader);
            _postFadeMaterial = new Material(_config.postFadeShader);
        }

        public void Sample(Texture sunTexture)
        {
            int textureWidth = _wallTexture.width;
            int textureHeight = _wallTexture.height;
            
            RenderTexture tempTexture = UnityEngine.RenderTexture.GetTemporary(
                textureWidth, 
                textureHeight, 
                0, 
                RenderTextureFormat.R16);
            
            _ambientMaterial.SetTexture(ShaderProperty.Ambient.WallTexture, _wallTexture);
            _ambientMaterial.SetTexture(ShaderProperty.Ambient.SunTexture, sunTexture);
            _ambientMaterial.SetTexture(ShaderProperty.Ambient.StateTexture, _stateTexture);
            _ambientMaterial.SetFloat(ShaderProperty.Ambient.Blur, _config.ambientBlur);
            _ambientMaterial.SetFloat(ShaderProperty.Ambient.LinearFall, _config.ambientLinearFall);
            _ambientMaterial.SetFloat(ShaderProperty.Ambient.BilinearFall, _config.ambientBilinearFall);
            
            Graphics.Blit(null, tempTexture, _ambientMaterial);
            Graphics.Blit(tempTexture, _stateTexture);
            UnityEngine.RenderTexture.ReleaseTemporary(tempTexture);
            
            RenderTexture mergedTexture = UnityEngine.RenderTexture.GetTemporary(
                textureWidth * 2, 
                textureHeight * 2, 
                0, 
                RenderTextureFormat.R16);
            
            Graphics.Blit(_stateTexture, mergedTexture, _postMergeMaterial);

            RenderTexture fadedTexture = UnityEngine.RenderTexture.GetTemporary(
                textureWidth * 2,
                textureHeight * 2,
                0,
                RenderTextureFormat.R16);
            
            _postFadeMaterial.SetTexture(ShaderProperty.WarFogFadePost.StateTexture, stateTextureWithPost);
            _postFadeMaterial.SetTexture(ShaderProperty.WarFogFadePost.SunTexture, mergedTexture);
            _postFadeMaterial.SetFloat(ShaderProperty.WarFogFadePost.FadeDelta, _config.fadeDelta);
            
            Graphics.Blit(null, fadedTexture, _postFadeMaterial);
            Graphics.Blit(fadedTexture, stateTextureWithPost);
            
            UnityEngine.RenderTexture.ReleaseTemporary(mergedTexture);
            UnityEngine.RenderTexture.ReleaseTemporary(fadedTexture);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Object.Destroy(_wallTexture);
                Object.Destroy(_ambientMaterial);
                Object.Destroy(_postMergeMaterial);
                Object.Destroy(_postFadeMaterial);
                Object.Destroy(stateTextureWithPost);
                Object.Destroy(_stateTexture);

                _disposed = true;
            }
        }
    }
}