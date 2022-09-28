using UnityEngine;

namespace ProjectTolerance
{
    public static class ShaderPropertyId
    {
        public static readonly int WallCutAlpha = Shader.PropertyToID("_WallCutAlpha");
        public static readonly int InteriorExterior = Shader.PropertyToID("_InteriorExterior");
    }
}