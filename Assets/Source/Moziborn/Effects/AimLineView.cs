using UnityEngine;

namespace Moziborn.Effects
{
    public class AimLineView
    {
        private readonly AimLinePrefab _instance;
        private readonly MaterialPropertyBlock _materialProperty;
        private readonly float _colorDitherMax;

        public AimLineView(AimLinePrefab instance)
        {
            _instance = instance;
            _materialProperty = new MaterialPropertyBlock();
            _colorDitherMax = instance.meshRenderer.sharedMaterial.GetFloat("_ColorDither");
        }

        public void SetLength(float length)
        {
            _instance.transform.localScale = new Vector3(1, 1, length);
        }

        public void SetTransparency(float alpha01)
        {
            _materialProperty.SetFloat("_ColorDither", alpha01 * _colorDitherMax);
            _instance.meshRenderer.SetPropertyBlock(_materialProperty);
        }

        public void SetColorBlend(float blend01)
        {
            _materialProperty.SetFloat("_ColorBlend", blend01);
            _instance.meshRenderer.SetPropertyBlock(_materialProperty);
        }
    }
}