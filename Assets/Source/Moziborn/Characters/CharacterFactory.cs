using UnityEngine;

namespace Moziborn.Characters
{
    public class CharacterFactory
    {
        private readonly CharacterFactoryConfig _config;

        public CharacterFactory(CharacterFactoryConfig config)
        {
            _config = config;
        }
        
        public CharacterView CreateView(CharacterMeshPrefab meshPrefab)
        {
            CharacterView view = Object.Instantiate(_config.characterPrefab);
            
            Object.Instantiate(meshPrefab, view.animator.transform);

            view.physicsCollider.radius = meshPrefab.radius;
            view.physicsCollider.height = meshPrefab.height;
            view.physicsCollider.center = new Vector3(0, meshPrefab.height * 0.5f, 0);

            return view;
        }
    }
}