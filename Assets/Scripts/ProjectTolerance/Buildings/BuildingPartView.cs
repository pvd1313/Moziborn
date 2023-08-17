using UnityEngine;

namespace ProjectTolerance.Buildings
{
    public class BuildingPartView : MonoBehaviour
    {
        public EBuildingPartType partType => _partType;
        
        [SerializeField] private EBuildingPartType _partType;
    }
}