using System.Collections.Generic;
using ProjectTolerance.Cinematics;
using UnityEngine;

namespace ProjectTolerance.Rendering.ScreenSpace
{
    public struct ScreenSpaceRenderSetup
    {
        public string renderName;
        public Shader postShader;
        public IReadOnlyList<int> postMatrix;
        public int textureResolution;
        public RenderTextureFormat textureFormat;
        public FilterMode textureFilterMode;
        public float cameraDepth;
        public int cameraCullingMask;
        public MainCameraController mainCamera;
    }
}