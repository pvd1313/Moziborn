using UnityEngine;

namespace ProjectTolerance
{
    public class BuildingPartData : MonoBehaviour
    {
        public EBuildingPartType PartType => _partType;
        
        [SerializeField] private EBuildingPartType _partType;
    }
}