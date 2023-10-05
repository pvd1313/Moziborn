using System.Collections;
using ProjectTolerance.Buildings;
using UnityEngine;

namespace ProjectTolerance.Rendering.AmbientLight
{
    public class AmbientLightController
    {
        public Texture2D wallTexture { get; private set; }
        public BoundsInt textureBounds { get; private set; }
        
        private readonly AmbientLightConfig _config;
        private readonly BuildingController _buildingController;
        
        private Texture2D _sunTexture;
        private Material _ambientLightMaterial;
        private RenderTexture _stateTexture;

        public AmbientLightController(AmbientLightConfig config, BuildingController buildingController)
        {
            _config = config;
            _buildingController = buildingController;
        }

        public IEnumerator Initialize()
        {
            BoundsInt bounds = _buildingController.levelBounds;

            _sunTexture = new Texture2D(bounds.size.x, bounds.size.y, TextureFormat.R16, false, true)
            {
                name = "AmbientLight_Sun",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            wallTexture = new Texture2D(bounds.size.x, bounds.size.y, TextureFormat.R16, false, true)
            {
                name = "AmbientLight_Wall",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            _stateTexture = new RenderTexture(bounds.size.x, bounds.size.y, 0, RenderTextureFormat.R16)
            {
                name = "AmbientLight_State",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            textureBounds = bounds;
            
            _ambientLightMaterial = new Material(_config.ambientLightShader);
            _ambientLightMaterial.SetTexture(ShaderPropertyId.Ambient.WallTexture, wallTexture);
            _ambientLightMaterial.SetTexture(ShaderPropertyId.Ambient.SunTexture, _sunTexture);
            _ambientLightMaterial.SetTexture(ShaderPropertyId.Ambient.StateTexture, _stateTexture);
            
            Shader.SetGlobalVector(
                ShaderPropertyId.AmbientLightMapOffset, 
                new Vector4(textureBounds.position.x, 0, textureBounds.position.y) * -1);
            
            Shader.SetGlobalVector(
                ShaderPropertyId.AmbientLightMapWorldSize, 
                new Vector4(textureBounds.size.x, textureBounds.size.y));
            
            Shader.SetGlobalTexture(ShaderPropertyId.AmbientLightMapTexture, _stateTexture);

            foreach (BuildingData building in _buildingController.buildings)
                foreach (BoundsInt room in building.rooms)
                    foreach (Vector3Int pos in room.allPositionsWithin)
                    {
                        Vector3Int localPoint = pos - bounds.position;
                        _sunTexture.SetPixel(localPoint.x, localPoint.y, Color.black);
                    }

            for (int x = 0; x < textureBounds.size.x; x++)
                for (int y = 0; y < textureBounds.size.y; y++)
                    wallTexture.SetPixel(x, y, Color.black);

            foreach (BuildingData building in _buildingController.buildings)
                foreach (Vector2Int wallPos in building.walls)
                {
                    Vector2Int localPoint = wallPos - new Vector2Int(bounds.position.x, bounds.position.y);
                    wallTexture.SetPixel(localPoint.x, localPoint.y, Color.white);
                }
            
            wallTexture.Apply();
            _sunTexture.Apply();

            yield break;
        }

        public void Update()
        {
            foreach (BuildingData building in _buildingController.buildings)
                foreach (DoorData door in building.doors)
                {
                    Vector2Int pos = door.position - new Vector2Int(textureBounds.position.x, textureBounds.position.y);
                    wallTexture.SetPixel(pos.x, pos.y, Color.white * door.view.doorState);
                }

            wallTexture.Apply();
        
            RenderTexture tempTexture = RenderTexture.GetTemporary(
                textureBounds.size.x, 
                textureBounds.size.y, 
                0, 
                RenderTextureFormat.R16);

            _ambientLightMaterial.SetFloat(ShaderPropertyId.Ambient.Blur, _config.blur);
            _ambientLightMaterial.SetFloat(ShaderPropertyId.Ambient.LinearFall, _config.linearFall);
            _ambientLightMaterial.SetFloat(ShaderPropertyId.Ambient.BilinearFall, _config.bilinearFall);
        
            Graphics.Blit(_stateTexture, tempTexture, _ambientLightMaterial);
            
            Graphics.Blit(tempTexture, _stateTexture);

            RenderTexture.ReleaseTemporary(tempTexture);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Bounds bounds = TileToWorldBounds(textureBounds);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        private Bounds TileToWorldBounds(BoundsInt bounds)
        {
            Vector3 center = new (bounds.center.x, 0, bounds.center.y);
            Vector3 size = new (bounds.size.x, 0, bounds.size.y);
            return new Bounds(center, size);
        }
    }
}