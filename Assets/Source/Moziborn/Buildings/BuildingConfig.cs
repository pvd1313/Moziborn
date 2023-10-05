using UnityEngine;

namespace Moziborn.Buildings
{
    public class BuildingConfig : ScriptableObject
    {
        public LevelView levelViewPrefab;
        public float transitionDuration;
        public Vector2 verticalCutRange;
    }
}