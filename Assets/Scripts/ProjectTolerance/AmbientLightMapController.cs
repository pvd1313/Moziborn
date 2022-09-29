using UnityEngine;

namespace ProjectTolerance
{
    public class AmbientLightMapController : MonoBehaviour
    {
        [SerializeField] private Texture2D _bakedAmbientMap;
        
        private Texture2D _dynamicAmbientMap;

        private void Awake()
        {
            _dynamicAmbientMap = new Texture2D(
                _bakedAmbientMap.width,
                _bakedAmbientMap.height,
                TextureFormat.R8, 
                false, 
                true)
            {
                filterMode = FilterMode.Bilinear,
                wrapModeU = TextureWrapMode.Clamp,
                wrapModeV = TextureWrapMode.Clamp
            };

            _dynamicAmbientMap.SetPixels(_bakedAmbientMap.GetPixels());
            _dynamicAmbientMap.Apply();
            
            Shader.SetGlobalVector(ShaderPropertyId.AmbientLightMapOffset, -transform.position);
            Shader.SetGlobalTexture(ShaderPropertyId.AmbientLightMapTexture, _dynamicAmbientMap);
            Shader.SetGlobalVector(ShaderPropertyId.AmbientLightMapSize, new Vector4(_dynamicAmbientMap.width, _dynamicAmbientMap.height));
        }

        public void ModifyPixel(int x, int y, float colorR8)
        {
            _dynamicAmbientMap.SetPixel(x, y, new Color(colorR8, 0, 0));
        }

        private void Update()
        {
            _dynamicAmbientMap.Apply();
        }
    }
}