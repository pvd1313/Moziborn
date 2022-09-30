using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectTolerance
{
    public class BuildingController : MonoBehaviour
    {
        private class BuildingTransition
        {
            public float targetAlpha;
            public float alpha;
            public MaterialPropertyBlock materialProperty;
        }
        
        [SerializeField] private Tilemap _level0Tilemap;
        [SerializeField] private Tilemap _level1Tilemap;
        [SerializeField] private float _transitionDuration;
        [SerializeField] private Collider _playerCollider;
        [SerializeField] private Transform _cameraTransform;

        private List<BuildingData> _buildings;
        private BuildingTransition[] _transitions;
        private bool _intialized;

        private IEnumerator Start()
        {
            // Wait for tilemap GameObjects be spawned.
            yield return null;
            
            Initialize();
        }
        
        private void Initialize()
        {
            _buildings = new List<BuildingData>();
            BuildingUtil.DetectBuildings(_level0Tilemap, _level1Tilemap, _buildings);

            _transitions = new BuildingTransition[_buildings.Count];
            for (var i = 0; i < _transitions.Length; i++)
            {
                var propertyBlock = new MaterialPropertyBlock();
                
                propertyBlock.SetFloat(ShaderPropertyId.VerticalCutAlpha, 1);
                
                _transitions[i] = new BuildingTransition
                {
                    targetAlpha = 1,
                    alpha = 1,
                    materialProperty = propertyBlock
                };

                foreach (var renderer in _buildings[i].viewBlockingParts)
                {
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }

            _intialized = true;
        }

        private void Update()
        {
            if (!_intialized)
            {
                return;
            }

            var cameraPos = _cameraTransform.position;
            var playerBounds = _playerCollider.bounds;
            var playerBottomRay = new Ray(cameraPos, playerBounds.min - cameraPos);

            for (var i = 0; i < _buildings.Count; i++)
            {
                var blocksView = false;
                foreach (var room in _buildings[i].rooms)
                {
                    if (room.IntersectRay(playerBottomRay))
                    {
                        blocksView = true;
                        break;
                    }
                }
                
                var transition = _transitions[i];

                transition.targetAlpha = blocksView ? 0 : 1;
                
                if (transition.alpha > transition.targetAlpha || transition.alpha < transition.targetAlpha)
                {
                    transition.alpha = Mathf.MoveTowards(transition.alpha,
                        transition.targetAlpha,
                        Time.deltaTime / _transitionDuration);
                    
                    transition.materialProperty.SetFloat(ShaderPropertyId.VerticalCutAlpha, transition.alpha);

                    foreach (var part in _buildings[i].viewBlockingParts)
                    {
                        part.SetPropertyBlock(transition.materialProperty);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_buildings == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            
            foreach (var building in _buildings)
            {
                foreach (var room in building.rooms)
                {
                    Gizmos.DrawWireCube(room.center, room.size);
                }
            }
        }
    }
}