using UnityEngine;

namespace Moziborn.Rendering
{
    public readonly struct ShaderProperties
    {
        private readonly Material _material;
        private readonly MaterialPropertyBlock _propertyBlock;
        private readonly bool _isMaterial;
        
        public ShaderProperties(Material material)
        {
            _isMaterial = true;
            _material = material;
            _propertyBlock = null;
        }

        public ShaderProperties(MaterialPropertyBlock propertyBlock)
        {
            _isMaterial = false;
            _material = null;
            _propertyBlock = propertyBlock;
        }

        public void Apply(MeshRenderer meshRenderer)
        {
            if (_isMaterial)
            {
                meshRenderer.sharedMaterial = _material;
            }
            else
            {
                meshRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        public void SetFloat(int propertyID, float value)
        {
            if (_isMaterial)
            {
                _material.SetFloat(propertyID, value);
            }
            else
            {
                _propertyBlock.SetFloat(propertyID, value);
            }
        }

        public float GetFloat(int propertyID)
        {
            if (_isMaterial)
            {
                return _material.GetFloat(propertyID);
            }
            else
            {
                return _propertyBlock.GetFloat(propertyID);
            }
        }
    }
}