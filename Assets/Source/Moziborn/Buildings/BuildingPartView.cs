using UnityEngine;

namespace Moziborn.Buildings
{
    public class BuildingPartView : MonoBehaviour
    {
        public BuildingPartType partType => _partType;
        
        [SerializeField] private BuildingPartType _partType;
    }
}