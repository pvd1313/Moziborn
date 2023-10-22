using System.Collections.Generic;
using UnityEngine;

namespace Moziborn.Buildings
{
    public interface ILevelLayout
    {
        IReadOnlyList<BuildingData> buildings { get; }
        BoundsInt bounds { get; }
    }
}