using ProjectTolerance.Cinematics;
using UnityEngine;

namespace ProjectTolerance.Rendering.ScreenSpace
{
    public class ScreenSpaceRender
    {
        public ScreenSpaceRenderResult lastRenderResult { get; private set; }
        
        private RenderTexture _renderTexture;
        private Camera _renderCamera;
        private ScreenSpaceRenderSetup _setup;
        private MainCameraController _mainCamera;
        private Material _blurMaterial;
        
        public ScreenSpaceRender (ScreenSpaceRenderSetup setup)
        {
            _setup = setup;
            _mainCamera = setup.mainCamera;
            
            _renderCamera = new GameObject(setup.renderName + "Camera").AddComponent<Camera>();
            _renderCamera.orthographic = true;
            _renderCamera.backgroundColor = Color.black;
            _renderCamera.clearFlags = CameraClearFlags.SolidColor;
            _renderCamera.cullingMask = setup.cameraCullingMask;
            _renderCamera.depth = setup.cameraDepth;

            _blurMaterial = new Material(_setup.postShader);
        }

        public void BeginCameraRendering(Camera camera)
        {
            if (camera != _renderCamera)
            {
                return;
            }
            
            float screenRatio = Screen.width / (float) Screen.height;
            Vector2Int requiredTextureSize = default;
            
            if (screenRatio > 0)
            {
                requiredTextureSize.x = Mathf.CeilToInt(_setup.textureResolution * screenRatio);
                requiredTextureSize.y = _setup.textureResolution;
            }
            else
            {
                requiredTextureSize.x = _setup.textureResolution;
                requiredTextureSize.y = Mathf.CeilToInt(_setup.textureResolution / screenRatio);
            }
            
            Vector2 viewSize = ComputeViewSize(screenRatio, _mainCamera.orthographicSize, _mainCamera.rotationDeg.x) + Vector2.one;
            
            if (_renderTexture == null ||
                _renderTexture.width != requiredTextureSize.x ||
                _renderTexture.height != requiredTextureSize.y)
            {
                if (_renderTexture != null)
                {
                    _renderTexture.Release();
                }
                
                _renderTexture = new RenderTexture(requiredTextureSize.x, requiredTextureSize.y, 0, _setup.textureFormat)
                {
                    name = _setup.renderName,
                    filterMode = _setup.textureFilterMode
                };
                
                _renderCamera.orthographicSize = Mathf.Min(viewSize.x, viewSize.y);
                _renderCamera.targetTexture = _renderTexture;
                _renderCamera.aspect = viewSize.x / viewSize.y;
            }
            
            Vector3 cameraPosition = _mainCamera.pivot;
            cameraPosition.x = Mathf.RoundToInt(cameraPosition.x);
            cameraPosition.y = Mathf.RoundToInt(cameraPosition.y);
            cameraPosition.z = Mathf.RoundToInt(cameraPosition.z);
            
            _renderCamera.transform.position = cameraPosition + Vector3.up;
            _renderCamera.transform.rotation = Quaternion.Euler(90, _mainCamera.rotationDeg.y, 0);
            
            float mapRotationRad = _mainCamera.rotationDeg.y * Mathf.Deg2Rad;
            Vector3 viewNorth = Vector3.ProjectOnPlane(_mainCamera.lookDirection, Vector3.up).normalized;
            Vector3 viewEast = Vector3.Cross(Vector3.up, viewNorth);
            Vector3 bottomLeft = cameraPosition - viewNorth * viewSize.y - viewEast * viewSize.x;

            lastRenderResult = new ScreenSpaceRenderResult
            {
                texture = _renderTexture,
                textureRotationSinCosWorldSpace = new Vector2 (Mathf.Cos(mapRotationRad), Mathf.Sin(mapRotationRad)),
                textureOffsetWorldSpace = bottomLeft * -1,
                textureSizeWorldSpace = viewSize * 2
            };
        }

        public void EndCameraRendering(Camera camera)
        {
            if (camera != _renderCamera || _renderTexture == null)
            {
                return;
            }

            if (_setup.postMatrix.Count > 0)
            {
                RenderTexture tempTexture = RenderTexture.GetTemporary(
                    _renderTexture.width,
                    _renderTexture.height,
                    _renderTexture.depth,
                    _renderTexture.format);
            
                RenderTexture sourceTexture = _renderTexture;
                RenderTexture destTexture = tempTexture;
            
                foreach (int blurPixelSize in _setup.postMatrix)
                {
                    _blurMaterial.SetFloat("_PixelOffset", blurPixelSize);
                    _blurMaterial.mainTexture = sourceTexture;

                    Graphics.Blit(sourceTexture, destTexture, _blurMaterial);

                    (sourceTexture, destTexture) = (destTexture, sourceTexture);
                }

                if (_setup.postMatrix.Count % 2 == 1)
                {
                    Graphics.Blit(sourceTexture, _renderTexture);
                }

                RenderTexture.ReleaseTemporary(tempTexture);
            }
        }
        
        private static Vector2 ComputeViewSize(float screenRatio, float orthographicSize, float pitchAngleDegrees)
        {
            return new Vector2
            {
                x = orthographicSize * screenRatio,
                y = orthographicSize / Mathf.Cos((90 - pitchAngleDegrees) * Mathf.Deg2Rad)
            };
        }
    }
}