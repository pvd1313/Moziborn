using UnityEngine;

namespace ProjectTolerance.Buildings
{
    public class BuildingPartData : MonoBehaviour
    {
        public EBuildingPartType PartType => _partType;
        
        [SerializeField] private EBuildingPartType _partType;
    }
}