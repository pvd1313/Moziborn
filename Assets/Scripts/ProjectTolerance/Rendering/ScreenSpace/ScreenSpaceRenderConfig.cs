using System.Collections.Generic;
using ProjectTolerance.Cinematics;
using UnityEngine;

namespace ProjectTolerance.Rendering.ScreenSpace
{
    public struct ScreenSpaceRenderConfig
    {
        public string renderName;
        public Shader blurShader;
        public IReadOnlyList<int> blurMatrix;
        public int textureResolution;
        public RenderTextureFormat textureFormat;
        public FilterMode textureFilterMode;
        public float cameraDepth;
        public int cameraCullingMask;
        public MainCameraController mainCamera;
    }
}