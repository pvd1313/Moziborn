using System;
using System.Linq;
using UnityEngine;

namespace ProjectTolerance
{
    public class BuildingController : MonoBehaviour
    {
        [Serializable]
        public struct BuildingParts
        {
            public Renderer[] renderers;
            public EBuildingPartType partType;
        }

        private class BuildingPartsTransition
        {
            public Renderer[] renderers;
            public MaterialPropertyBlock propertyBlock;
            public EBuildingPartType partType;
            public float alpha;
            public float targetAlpha;
        }

        public Bounds BuildingBounds => _buildingBoundsCollider.bounds;
        public Bounds RoomBounds => _roomBoundsCollider.bounds;
        
        [SerializeField] private BoxCollider _buildingBoundsCollider;
        [SerializeField] private BoxCollider _roomBoundsCollider;
        [SerializeField] private BuildingParts[] _parts;

        private BuildingPartsTransition[] _transitions;
        private float _transitionDuration;

        private void Awake()
        {
            _transitions = _parts.Select(p => new BuildingPartsTransition
            {
                renderers = p.renderers,
                partType = p.partType,
                alpha = 1,
                targetAlpha = 1,
                propertyBlock = new MaterialPropertyBlock()
            }).ToArray();
        }

        private void Update()
        {
            foreach (var transition in _transitions)
            {
                if (transition.targetAlpha > transition.alpha || transition.targetAlpha < transition.alpha)
                {
                    transition.alpha = Mathf.MoveTowards(transition.alpha,
                        transition.targetAlpha,
                        Time.deltaTime / _transitionDuration);
                    
                    transition.propertyBlock.SetFloat("_WallCutAlpha", transition.alpha);
                    
                    foreach (var transitionRenderer in transition.renderers)
                    {
                        transitionRenderer.SetPropertyBlock(transition.propertyBlock);
                    }
                }
            }
        }

        public void SwitchVisibleParts(EBuildingPartType visibleParts, float transitionDuration)
        {
            _transitionDuration = transitionDuration;
            
            foreach (var transition in _transitions)
            {
                transition.targetAlpha = visibleParts.HasFlag(transition.partType) ? 1 : 0;
            }
        }
    }
}