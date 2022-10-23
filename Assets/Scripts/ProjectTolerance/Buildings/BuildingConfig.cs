using System;

namespace ProjectTolerance.Buildings
{
    [Serializable]
    public struct BuildingConfig
    {
        public LevelView levelViewPrefab;
        public float transitionDuration;
    }
}