using System.Collections.Generic;
using UnityEngine;

namespace Moziborn.Buildings
{
    public class BuildingPartView : MonoBehaviour
    {
        public BuildingPartType partType => _partType;
        public IReadOnlyList<MeshRenderer> meshRenders => _meshRenders;
        
        [SerializeField] private BuildingPartType _partType;
        [SerializeField] private MeshRenderer[] _meshRenders;
    }
}