using System;
using UnityEngine;

namespace ProjectTolerance
{
    public class InteriorExteriorLayerSwitcher : MonoBehaviour
    {
        [SerializeField] private BuildingController _buildingController;
        [SerializeField] private Collider _collider;
        [SerializeField] private Renderer _renderer;

        private MaterialPropertyBlock _materialProperty;

        private void Awake()
        {
            _materialProperty = new MaterialPropertyBlock();
        }

        private void Update()
        {
            var playerVolume = GetBoundsVolume(_collider.bounds);
            var overlapVolume = GetOverlapVolume(_collider.bounds, _buildingController.RoomBounds);
            var playerInside = overlapVolume / playerVolume;
            
            gameObject.layer = playerInside == 0 ? LayerNames.Default : LayerNames.Interior;
            _materialProperty.SetFloat(ShaderPropertyId.InteriorExterior, 1 - playerInside);
            _renderer.SetPropertyBlock(_materialProperty);
        }

        private float GetOverlapVolume(Bounds a, Bounds b)
        {
            if (!a.Intersects(b))
            {
                return 0;
            }
            
            var minA = a.min;
            var maxA = a.max;
            var minB = b.min;
            var maxB = b.max;

            var min = new Vector3
            {
                x = Mathf.Max(minA.x, minB.x),
                y = Mathf.Max(minA.y, minB.y),
                z = Mathf.Max(minA.z, minB.z)
            };
            
            var max = new Vector3
            {
                x = Mathf.Min(maxA.x, maxB.x),
                y = Mathf.Min(maxA.y, maxB.y),
                z = Mathf.Min(maxA.z, maxB.z)
            };
            
            var diagonal = max - min;
            
            return diagonal.x * diagonal.y * diagonal.z;
        }

        private float GetBoundsVolume(Bounds bounds)
        {
            var diagonal = bounds.max - bounds.min;
            
            return diagonal.x * diagonal.y * diagonal.z;
        }
    }
}