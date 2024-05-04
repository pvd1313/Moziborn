using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moziborn.Characters
{
    public class CharacterFactory
    {
        private readonly CharacterFactoryConfig _config;
        
        private Dictionary<string, CharacterMeshPrefab> _meshPrefabs;

        public CharacterFactory(CharacterFactoryConfig config)
        {
            _config = config;
        }

        public IEnumerator Initialize()
        {
            _meshPrefabs = _config.characterMeshPrefabs.ToDictionary(c => c.id);

            yield return null;
        }
        
        public CharacterView CreateView(string meshId)
        {
            if (!_meshPrefabs.TryGetValue(meshId, out CharacterMeshPrefab meshPrefab))
            {
                throw new InvalidOperationException($"Unknown character mesh '{meshId}'");
            }

            CharacterPrefab character = GameObject.Instantiate(_config.characterPrefab);
            CharacterMeshPrefab mesh = GameObject.Instantiate(meshPrefab);

            return new CharacterView(character, mesh);
        }
    }
}