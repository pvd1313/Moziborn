using System.Collections;
using Moziborn.Buildings;
using UnityEngine;

namespace Moziborn.Rendering.AmbientLight
{
    public class AmbientLightRender
    {
        private readonly AmbientLightConfig _config;
        private readonly BuildingController _buildingController;
        
        private Texture2D _wallTexture;
        private Texture2D _sunTexture;
        private Material _ambientLightMaterial;
        private RenderTexture _stateTexture;

        public AmbientLightRender(AmbientLightConfig config, BuildingController buildingController)
        {
            _config = config;
            _buildingController = buildingController;
        }

        public IEnumerator Initialize()
        {
            BoundsInt levelBounds = _buildingController.bounds;

            _sunTexture = new Texture2D(levelBounds.size.x, levelBounds.size.y, TextureFormat.R16, false, true)
            {
                name = "AmbientLight_Sun",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            _wallTexture = new Texture2D(levelBounds.size.x, levelBounds.size.y, TextureFormat.R16, false, true)
            {
                name = "AmbientLight_Wall",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            _stateTexture = new RenderTexture(levelBounds.size.x, levelBounds.size.y, 0, RenderTextureFormat.R16)
            {
                name = "AmbientLight_State",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            
            _ambientLightMaterial = new Material(_config.ambientLightShader);
            _ambientLightMaterial.SetTexture(ShaderProperty.Ambient.WallTexture, _wallTexture);
            _ambientLightMaterial.SetTexture(ShaderProperty.Ambient.SunTexture, _sunTexture);
            _ambientLightMaterial.SetTexture(ShaderProperty.Ambient.StateTexture, _stateTexture);
            
            Shader.SetGlobalVector(
                ShaderProperty.Global.AmbientLightMapOffset, 
                new Vector4(levelBounds.position.x, 0, levelBounds.position.y) * -1);
            
            Shader.SetGlobalVector(
                ShaderProperty.Global.AmbientLightMapWorldSize, 
                new Vector4(levelBounds.size.x, levelBounds.size.y));
            
            Shader.SetGlobalTexture(ShaderProperty.Global.AmbientLightMapTexture, _stateTexture);
            
            _sunTexture.Fill(Color.white);
            _sunTexture.FillRooms(_buildingController, Color.black);
            _sunTexture.Apply();
            
            _wallTexture.Fill(Color.black);
            _wallTexture.DrawWalls(_buildingController, Color.white);
            _wallTexture.Apply();

            yield break;
        }

        public void Update()
        {
            BoundsInt levelBounds = _buildingController.bounds;
            
            _wallTexture.DrawDoors(_buildingController);
            _wallTexture.Apply();
        
            RenderTexture tempTexture = RenderTexture.GetTemporary(
                levelBounds.size.x, 
                levelBounds.size.y, 
                0, 
                RenderTextureFormat.R16);

            _ambientLightMaterial.SetFloat(ShaderProperty.Ambient.Blur, _config.blur);
            _ambientLightMaterial.SetFloat(ShaderProperty.Ambient.LinearFall, _config.linearFall);
            _ambientLightMaterial.SetFloat(ShaderProperty.Ambient.BilinearFall, _config.bilinearFall);
        
            Graphics.Blit(_stateTexture, tempTexture, _ambientLightMaterial);
            
            Graphics.Blit(tempTexture, _stateTexture);

            RenderTexture.ReleaseTemporary(tempTexture);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Bounds bounds = TileToWorldBounds(_buildingController.bounds);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        private static Bounds TileToWorldBounds(BoundsInt bounds)
        {
            Vector3 center = new (bounds.center.x, 0, bounds.center.y);
            Vector3 size = new (bounds.size.x, 0, bounds.size.y);
            return new Bounds(center, size);
        }
    }
}