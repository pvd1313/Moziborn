using System;
using UnityEngine;

namespace ProjectTolerance
{
    public class InteriorExteriorLayerSwitcher : MonoBehaviour
    {
        [SerializeField] private BuildingController _buildingController;

        private bool _onDefaultLayer;

        private void Awake()
        {
            _onDefaultLayer = true;
        }

        private void Update()
        {
            var onDefaultLayer = !_buildingController.RoomBounds.Contains(transform.position);

            if (onDefaultLayer != _onDefaultLayer)
            {
                _onDefaultLayer = onDefaultLayer;
                gameObject.layer = _onDefaultLayer ? LayerNames.Default : LayerNames.Interior;
            }
        }
    }
}