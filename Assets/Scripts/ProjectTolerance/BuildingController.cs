using System;
using System.Linq;
using UnityEngine;

namespace ProjectTolerance
{
    public class BuildingController : MonoBehaviour
    {
        [Serializable]
        public struct BuildingPart
        {
            public Renderer renderer;
            public EBuildingPartLocation location;
            public EBuildingPartType type;
            public bool isExterior;
        }

        private class BuildingPartTransition
        {
            public Renderer renderer;
            public MaterialPropertyBlock propertyBlock;
            public EBuildingPartLocation location;
            public EBuildingPartType type;
            public float alpha;
            public float targetAlpha;
            public bool isExterior;
        }

        public Bounds BuildingBounds => _buildingBoundsCollider.bounds;
        public Bounds RoomBounds => _roomBoundsCollider.bounds;
        
        [SerializeField] private BoxCollider _buildingBoundsCollider;
        [SerializeField] private BoxCollider _roomBoundsCollider;
        [SerializeField] private BuildingPart[] _parts;

        private BuildingPartTransition[] _transitions;
        private float _transitionDuration;

        private void Awake()
        {
            _transitions = _parts.Select(p => new BuildingPartTransition
            {
                renderer = p.renderer,
                location = p.location,
                type = p.type,
                alpha = 1,
                targetAlpha = 1,
                isExterior = p.isExterior,
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
                    
                    transition.propertyBlock.SetFloat(ShaderPropertyId.WallCutAlpha, transition.alpha);
                    transition.propertyBlock.SetFloat(ShaderPropertyId.InteriorExterior, transition.isExterior ? 1 : 0);
                    
                    transition.renderer.SetPropertyBlock(transition.propertyBlock);
                }
            }
        }

        public void SwitchVisibleParts(EBuildingPartLocation visibleParts, float transitionDuration)
        {
            _transitionDuration = transitionDuration;
            
            foreach (var transition in _transitions)
            {
                if (transition.type != EBuildingPartType.Door)
                {
                    transition.targetAlpha = visibleParts.HasFlag(transition.location) ? 1 : 0;
                }
            }
        }

        public void SetExteriorDoorsInteriorExterior(float value)
        {
            foreach (var part in _transitions)
            {
                if (part.type == EBuildingPartType.Door && part.isExterior)
                {
                    part.propertyBlock.SetFloat(ShaderPropertyId.InteriorExterior, value);
                    part.renderer.SetPropertyBlock(part.propertyBlock);
                }
            }
        }
        
        public void SetInteriorBulbsVisible(bool visible)
        {
            foreach (var part in _parts)
            {
                if (!part.isExterior && part.type == EBuildingPartType.Bulb)
                {
                    part.renderer.gameObject.SetActive(visible);
                }
            }
        }
    }
}