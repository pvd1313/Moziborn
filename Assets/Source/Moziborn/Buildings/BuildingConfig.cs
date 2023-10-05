using System;
using UnityEngine;

namespace Moziborn.Buildings
{
    [Serializable]
    public class BuildingConfig
    {
        public LevelView levelViewPrefab;
        public float transitionDuration;
        public Vector2 verticalCutRange;
    }
}