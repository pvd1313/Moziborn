using System.Collections;
using ProjectTolerance.Buildings;
using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Shading.Lighting
{
    public class AmbientLightController : IUnorderedLateUpdatable
    {
        private readonly AmbientLightConfig _config;
        private BuildingController _buildingController;
        
        private Texture2D _sunLightTexture;
        private Texture2D _wallsTexture;
        private BoundsInt _textureBounds;
        private Material _ambientLightMaterial;
        private RenderTexture _stateTexture;

        public AmbientLightController(AmbientLightConfig config, BuildingController buildingController)
        {
            _config = config;
            _buildingController = buildingController;
        }

        public IEnumerator Initialize()
        {
            BoundsInt bounds = new (Vector3Int.zero, Vector3Int.zero);
            
            if (_buildingController.buildings.Count > 0)
            {
                bounds = _buildingController.buildings[0].rooms[0];
                
                foreach (BuildingData building in _buildingController.buildings)
                    foreach (BoundsInt room in building.rooms)
                        bounds = JoinBounds(room, bounds);
            }

            bounds.min -= new Vector3Int(1, 1, 0);
            bounds.max += new Vector3Int(1, 1, 0);

            _sunLightTexture = new Texture2D(bounds.size.x, bounds.size.y, TextureFormat.R8, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            _wallsTexture = new Texture2D(bounds.size.x, bounds.size.y, TextureFormat.R8, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            _stateTexture = new RenderTexture(bounds.size.x, bounds.size.y, 0, RenderTextureFormat.R8)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            _textureBounds = bounds;
            
            _ambientLightMaterial = new Material(_config.ambientLightShader);
            _ambientLightMaterial.SetTexture("_WallsTexture", _wallsTexture);
            _ambientLightMaterial.SetTexture("_SunLightTexture", _sunLightTexture);
            _ambientLightMaterial.SetTexture("_StateTexture", _stateTexture);
            _ambientLightMaterial.SetVector("_TextureSize", new Vector4(_textureBounds.size.x, _textureBounds.size.y));
            
            Shader.SetGlobalVector(ShaderPropertyId.AmbientLightMapOffset, 
                new Vector4(_textureBounds.position.x, 0, _textureBounds.position.y) * -1);
            
            Shader.SetGlobalVector(ShaderPropertyId.AmbientLightMapWorldSize, 
                new Vector4(_textureBounds.size.x, _textureBounds.size.y));
            
            Shader.SetGlobalTexture(ShaderPropertyId.AmbientLightMapTexture, _stateTexture);
            
            foreach (BuildingData building in _buildingController.buildings)
                foreach (BoundsInt room in building.rooms)
                    foreach (Vector3Int pos in room.allPositionsWithin)
                    {
                        Vector3Int localPoint = pos - bounds.position;
                        _sunLightTexture.SetPixel(localPoint.x, localPoint.y, Color.black);
                    }

            for (int x = 0; x < _textureBounds.size.x; x++)
                for (int y = 0; y < _textureBounds.size.y; y++)
                    _wallsTexture.SetPixel(x, y, Color.black);
            
            foreach (BuildingData building in _buildingController.buildings)
                foreach (Vector2Int wallPos in building.walls)
                {
                    Vector2Int localPoint = wallPos - new Vector2Int(bounds.position.x, bounds.position.y);
                    _wallsTexture.SetPixel(localPoint.x, localPoint.y, Color.white);
                }
            
            _wallsTexture.Apply();
            _sunLightTexture.Apply();

            yield break;
        }
        
        public void LateUpdate()
        {
            foreach (BuildingData building in _buildingController.buildings)
                foreach (DoorData door in building.doors)
                {
                    Vector2Int pos = door.position - new Vector2Int(_textureBounds.position.x, _textureBounds.position.y);
                    _wallsTexture.SetPixel(pos.x, pos.y, Color.white * door.view.doorState);
                }
            _wallsTexture.Apply();
        
            RenderTexture tempTexture = RenderTexture.GetTemporary(
                _textureBounds.size.x, 
                _textureBounds.size.y, 
                0, 
                RenderTextureFormat.R8);
        
            Graphics.Blit(_stateTexture, tempTexture, _ambientLightMaterial);
            
            Graphics.Blit(tempTexture, _stateTexture);
            
            RenderTexture.ReleaseTemporary(tempTexture);
        }

        private BoundsInt JoinBounds(BoundsInt a, BoundsInt b)
        {
            int xMin = Mathf.Min(a.min.x, b.min.x);
            int yMin = Mathf.Min(a.min.y, b.min.y);
            int zMin = Mathf.Min(a.min.z, b.min.z);
            int xMax = Mathf.Max(a.max.x, b.max.x);
            int yMax = Mathf.Max(a.max.y, b.max.y);
            int zMax = Mathf.Max(a.max.z, b.max.z);
            return new BoundsInt(xMin, yMin, zMin, xMax - xMin, yMax - yMin, zMax - yMin);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Bounds bounds = TileToWorldBounds(_textureBounds);
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