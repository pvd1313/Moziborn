using System.Collections;
using ProjectTolerance.Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectTolerance
{
    public class AmbientLightController : MonoBehaviour
    {
        [SerializeField] private BuildingController _buildingController;
        [SerializeField] private Shader _ambientLightShader;
        
        private Texture2D _sunLightTexture;
        private Texture2D _wallsTexture;
        private BoundsInt _textureBounds;
        private Material _ambientLightMaterial;
        private RenderTexture _stateTexture;

        private bool _initialized;

        public IEnumerator Initialize()
        {
            var bounds = new BoundsInt(Vector3Int.zero, Vector3Int.zero);
            
            if (_buildingController.Buildings.Count > 0)
            {
                bounds = _buildingController.Buildings[0].rooms[0];
                
                foreach (var building in _buildingController.Buildings)
                    foreach (var room in building.rooms)
                        bounds = JoinBounds(room, bounds);
            }

            bounds.min += new Vector3Int(-1, -1, 0);
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
            
            _ambientLightMaterial = new Material(_ambientLightShader);
            _ambientLightMaterial.SetTexture("_WallsTexture", _wallsTexture);
            _ambientLightMaterial.SetTexture("_SunLightTexture", _sunLightTexture);
            _ambientLightMaterial.SetTexture("_StateTexture", _stateTexture);
            _ambientLightMaterial.SetVector("_TextureSize", new Vector4(_textureBounds.size.x, _textureBounds.size.y));
            
            Shader.SetGlobalVector(ShaderPropertyId.AmbientLightMapOffset, 
                new Vector4(_textureBounds.position.x, 0, _textureBounds.position.y) * -1);
            
            Shader.SetGlobalVector(ShaderPropertyId.AmbientLightMapSize, 
                new Vector4(_textureBounds.size.x, _textureBounds.size.y));
            
            Shader.SetGlobalTexture(ShaderPropertyId.AmbientLightMapTexture, _stateTexture);
            
            foreach (var building in _buildingController.Buildings)
                foreach (var room in building.rooms)
                    foreach (var pos in room.allPositionsWithin)
                    {
                        var localPoint = pos - bounds.position;
                        _sunLightTexture.SetPixel(localPoint.x, localPoint.y, Color.black);
                    }

            for (var x = 0; x < _textureBounds.size.x; x++)
                for (var y = 0; y < _textureBounds.size.y; y++)
                    _wallsTexture.SetPixel(x, y, Color.black);
            
            foreach (var building in _buildingController.Buildings)
                foreach (var wallPos in building.walls)
                {
                    var localPoint = wallPos - new Vector2Int(bounds.position.x, bounds.position.y);
                    _wallsTexture.SetPixel(localPoint.x, localPoint.y, Color.white);
                }
            
            _wallsTexture.Apply();
            _sunLightTexture.Apply();

            _initialized = true;

            yield break;
        }
        
        private void LateUpdate()
        {
            if (!_initialized)
            {
                return;
            }

            foreach (var building in _buildingController.Buildings)
                foreach (var door in building.doors)
                {
                    var pos = door.position - new Vector2Int(_textureBounds.position.x, _textureBounds.position.y);
                    _wallsTexture.SetPixel(pos.x, pos.y, Color.white * door.controller.DoorState);
                }
            _wallsTexture.Apply();

            var tempTexture = RenderTexture.GetTemporary(
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
            var xMin = Mathf.Min(a.min.x, b.min.x);
            var yMin = Mathf.Min(a.min.y, b.min.y);
            var zMin = Mathf.Min(a.min.z, b.min.z);
            var xMax = Mathf.Max(a.max.x, b.max.x);
            var yMax = Mathf.Max(a.max.y, b.max.y);
            var zMax = Mathf.Max(a.max.z, b.max.z);
            return new BoundsInt(xMin, yMin, zMin, xMax - xMin, yMax - yMin, zMax - yMin);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var bounds = TileToWorldBounds(_textureBounds);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        private Bounds TileToWorldBounds(BoundsInt bounds)
        {
            var center = new Vector3(bounds.center.x, 0, bounds.center.y);
            var size = new Vector3(bounds.size.x, 0, bounds.size.y);
            return new Bounds(center, size);
        }
    }
}