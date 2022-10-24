using UnityEngine;

namespace ProjectTolerance.Buildings
{
    public class BuildingPartView : MonoBehaviour
    {
        public EBuildingPartType PartType => _partType;
        
        [SerializeField] private EBuildingPartType _partType;
    }
}