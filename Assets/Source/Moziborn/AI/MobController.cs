using System.Collections;
using UnityEngine;

namespace Moziborn.AI
{
    public class MobController
    {
        private readonly MobControllerConfig _config;

        private Transform _root;
        private NavigationSurfacePrefab _navigationSurface;

        public MobController(MobControllerConfig config)
        {
            _config = config;
        }

        public IEnumerator Initialize()
        {
            _root = new GameObject("MobController").transform;

            _navigationSurface = GameObject.Instantiate(_config.navigationSurfacePrefab, _root);
            _navigationSurface.navMeshSurface.BuildNavMesh();
            
            yield break;
        }
    }
}