using UnityEngine;

namespace Moziborn.Rendering.PointLight
{
    public readonly struct PointLightShaderProperties
    {
        private static int _SpreadAngleRad = Shader.PropertyToID("_SpreadAngleRad");
        private static int _MaxRayDistance = Shader.PropertyToID("_MaxRayDistance");
        private static int _Intensity = Shader.PropertyToID("_Intensity");
        private static int _StartOffset = Shader.PropertyToID("_StartOffset");
        private static int _HeadAngleRad = Shader.PropertyToID("_HeadAngleRad");

        public float spreadAngleRad
        {
            get => _properties.GetFloat(_SpreadAngleRad);
            set => _properties.SetFloat(_SpreadAngleRad, value);
        }

        public float maxRayDistance
        {
            get => _properties.GetFloat(_MaxRayDistance);
            set => _properties.SetFloat(_MaxRayDistance, value);
        }

        public float intensity
        {
            get => _properties.GetFloat(_Intensity);
            set => _properties.SetFloat(_Intensity, value);
        }

        public float startOffset
        {
            get => _properties.GetFloat(_StartOffset);
            set => _properties.SetFloat(_StartOffset, value);
        }

        public float headAngleRad
        {
            get => _properties.GetFloat(_HeadAngleRad);
            set => _properties.SetFloat(_HeadAngleRad, value);
        }
        
        private readonly ShaderProperties _properties;
        
        public PointLightShaderProperties(Material material)
        {
            _properties = new ShaderProperties(material);
        }

        public PointLightShaderProperties(MaterialPropertyBlock materialPropertyBlock)
        {
            _properties = new ShaderProperties(materialPropertyBlock);
        }

        public void Apply(MeshRenderer meshRenderer) => _properties.Apply(meshRenderer);
    }
}