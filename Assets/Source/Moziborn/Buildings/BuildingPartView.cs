using UnityEngine;

namespace Moziborn.Buildings
{
    public class BuildingPartView : MonoBehaviour
    {
        public EBuildingPartType partType => _partType;
        
        [SerializeField] private EBuildingPartType _partType;
    }
}