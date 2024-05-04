using UnityEngine;

namespace Moziborn.Characters
{
    public class CharacterFactoryConfig : ScriptableObject
    {
        public CharacterPrefab characterPrefab;
        public CharacterMeshPrefab[] characterMeshPrefabs;
    }
}