using UnityEngine;

namespace ProjectTolerance.Rendering.ScreenSpace
{
    public struct ScreenSpaceRenderResult
    {
        public RenderTexture texture;
        public Vector2 textureRotationSinCosWorldSpace;
        public Vector3 textureOffsetWorldSpace;
        public Vector2 textureSizeWorldSpace;
    }
}