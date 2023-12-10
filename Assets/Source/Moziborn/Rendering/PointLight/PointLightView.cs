using Moziborn.Layers;
using UnityEngine;

namespace Moziborn.Rendering.PointLight
{
    public class PointLightView : MonoBehaviour
    {
        private struct TorchRaycastHit
        {
            public int colliderInstanceId;
            public float rayTraveledDistance; 
            public Vector3 surfaceNormal;
            public Vector3 rayEndPoint;
        }

        private static readonly bool DrawGizmos = true;
        
        private const int BatchSize = 15;
        private const float MaxSamplesPerRad = 300;
        private const float SameSurfaceNormalDot = 0.0001f;
        private const float BatchSampleDistanceCheckMultiplier = 2f;
        
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        
        private float _spreadAngleRad;
        private float _torchRange;
        private float _torchRotationRad;
        private float _sampleDeltaAngleRad;
        private int _maxSampleCount;
        private Vector3 _torchCenter;
        private Transform _torchTransform;
        
        private Mesh _mesh;
        private Vector3[] _meshVertices;
        //private Vector2[] _meshUVs;
        private Color32[] _meshColors;
        private int[] _meshTriangles;

        public void Initialize(PointLightSetup lightSetup)
        {
            _meshRenderer.sharedMaterial = lightSetup.material;
            gameObject.layer = lightSetup.layerIndex;
            
            _torchTransform = transform;
            _maxSampleCount = Mathf.Max(2, Mathf.CeilToInt(lightSetup.spreadAngleRad * MaxSamplesPerRad));
            _spreadAngleRad = lightSetup.spreadAngleRad;
            _sampleDeltaAngleRad = _spreadAngleRad / (_maxSampleCount - 1);
            _torchRange = lightSetup.range;

            _mesh = new Mesh
            {
                name = "PointLight",
            };

            _meshVertices = new Vector3[_maxSampleCount + 1];
            //_meshUVs = new Vector2[_maxSampleCount + 1];
            _meshTriangles = new int[(_maxSampleCount - 1) * 3];
            _meshColors = new Color32[_maxSampleCount + 1];

            for (int i = 0; i < _maxSampleCount - 1; i++)
            {
                _meshTriangles[i * 3 + 0] = 0;
                _meshTriangles[i * 3 + 1] = i + 1;
                _meshTriangles[i * 3 + 2] = i + 2;
            }

            _meshVertices[0] = Vector3.zero;
            for (int i = 0; i < _maxSampleCount; i++)
            {
                _meshVertices[i + 1] = DirectionFromAngle(-0.5f * _spreadAngleRad + i * _sampleDeltaAngleRad);
            }

            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * lightSetup.range);
            _mesh.SetVertices(_meshVertices);
            _mesh.SetColors(_meshColors);
            //_mesh.SetUVs(0, _meshUVs);
            _mesh.SetTriangles(_meshTriangles, 0);
            _mesh.UploadMeshData(false);
            
            _meshFilter.mesh = _mesh;

            MaterialPropertyBlock propertyBlock = new ();
            propertyBlock.SetFloat("_MaxRayDistance", lightSetup.range);
            propertyBlock.SetFloat("_SpreadAngleRad", lightSetup.spreadAngleRad);
            _meshRenderer.SetPropertyBlock(propertyBlock);

            _torchCenter = lightSetup.position + Vector3.up * 0.1f;
            _torchRotationRad = lightSetup.rotationRad;
        }

        public void UpdatePositionRotation(Vector3 position, float rotationRad)
        {
            _torchCenter = position + Vector3.up * 0.1f;
            _torchRotationRad = rotationRad;
        }

        public void RebuildMesh()
        {
            const float HitPenetrationDistance = 0.02f;
            
            float torchRotationRad = Mathf.CeilToInt(_torchRotationRad / _sampleDeltaAngleRad) * _sampleDeltaAngleRad;
            float startAngleRad = -0.5f * _spreadAngleRad + torchRotationRad;

            Vector3 a = DirectionFromAngle(0) * _torchRange;
            Vector3 b = DirectionFromAngle(_sampleDeltaAngleRad * BatchSize) * _torchRange;
            float maxOptimizedBatchSampleDistance = (b - a).magnitude * BatchSampleDistanceCheckMultiplier;

            //_torchTransform.forward = DirectionFromAngle(torchRotationRad);
            _torchTransform.position = _torchCenter;

            _meshVertices[0] = Vector3.zero;
            
            Vector3 direction = DirectionFromAngle(startAngleRad);
            TorchRaycastHit prevHit = Raycast(_torchCenter, direction, _torchRange);
            _meshVertices[1] = prevHit.rayEndPoint - prevHit.surfaceNormal * HitPenetrationDistance - _torchCenter;

            if (DrawGizmos) Debug.DrawLine(_torchCenter, _torchCenter + _meshVertices[1], Color.red);

            for (int prevHitIndex = 0; prevHitIndex < _maxSampleCount - 1;)
            {
                int curHitIndex = Mathf.Min( _maxSampleCount - 1, prevHitIndex + BatchSize);
                int batchSize = curHitIndex - prevHitIndex;
                float angleRad = startAngleRad + _sampleDeltaAngleRad * curHitIndex;

                direction = DirectionFromAngle(angleRad);
                TorchRaycastHit curHit = Raycast(_torchCenter, direction, _torchRange);
                _meshVertices[curHitIndex + 1] = curHit.rayEndPoint - curHit.surfaceNormal * HitPenetrationDistance - _torchCenter;

                if (DrawGizmos) Debug.DrawLine(_torchCenter, _torchCenter + _meshVertices[curHitIndex + 1], Color.red);

                float sampleDistance = (prevHit.rayEndPoint - curHit.rayEndPoint).magnitude;

                if (sampleDistance <= maxOptimizedBatchSampleDistance && 
                    IsSameSurface(prevHit, curHit))
                {
                    if (prevHit.colliderInstanceId != 0)
                    {
                        Plane surface = new Plane(prevHit.surfaceNormal, prevHit.rayEndPoint);
                    
                        for (int i = 0; i < batchSize - 1; i++)
                        {
                            angleRad = startAngleRad + _sampleDeltaAngleRad * (prevHitIndex + i + 1);
                            direction = DirectionFromAngle(angleRad);
                            Ray ray = new (_torchCenter, direction);
                            surface.Raycast(ray, out float distance);
                            _meshVertices[prevHitIndex + i + 2] = ray.GetPoint(distance) - surface.normal * HitPenetrationDistance - _torchCenter;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < batchSize - 1; i++)
                        {
                            angleRad = startAngleRad + _sampleDeltaAngleRad * (prevHitIndex + i + 1);
                            direction = DirectionFromAngle(angleRad);
                            _meshVertices[prevHitIndex + i + 2] = _torchCenter + direction * _torchRange - _torchCenter;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < batchSize - 1; i++)
                    {
                        angleRad = startAngleRad + _sampleDeltaAngleRad * (prevHitIndex + i + 1);
                        direction = DirectionFromAngle(angleRad);
                        TorchRaycastHit curBatchHit = Raycast(_torchCenter, direction, _torchRange);
                        _meshVertices[prevHitIndex + i + 2] = curBatchHit.rayEndPoint - curBatchHit.surfaceNormal * HitPenetrationDistance - _torchCenter;

                        if (DrawGizmos) Debug.DrawLine(_torchCenter, _torchCenter + _meshVertices[prevHitIndex + i + 2], Color.cyan);
                    }
                }

                prevHitIndex = curHitIndex;
                prevHit = curHit;
            }

            for (int i = 1; i < _meshVertices.Length - 1; i++)
            {
                Vector3 direction0 = _meshVertices[i];
                Vector3 direction1 = _meshVertices[i + 1];
                
                int j = 1;
                while (Vector3.Cross(direction0, direction1).y < 0 && 
                       (j < i) && 
                       (j + i + 1 < _meshVertices.Length))
                {
                    direction0 = _meshVertices[i - j];
                    direction1 = _meshVertices[i + j + 1];
                    
                    j++;
                }
            
                if (j <= 1)
                {
                    continue;
                }
                
                j--;
                    
                Vector3 middlePosition = _meshVertices[i - j];
                    
                for (int t = i - j; t <= i + 1 + j; t++)
                {
                    _meshVertices[t] = middlePosition;
                }
                
                i += j;
            }
        
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * _torchRange);
            _mesh.SetVertices(_meshVertices);
            _mesh.UploadMeshData(false);
        }

        private static bool IsSameSurface(TorchRaycastHit a, TorchRaycastHit b)
        {
            if (a.colliderInstanceId == 0 && b.colliderInstanceId == 0)
            {
                return true;
            }

            if (a.colliderInstanceId == 0 || b.colliderInstanceId == 0)
            {
                return false;
            }
            
            Vector3 hitDirection = (b.rayEndPoint - a.rayEndPoint).normalized;
            Vector3 surfaceDirection = Vector3.Cross(a.surfaceNormal, Vector3.up);
            float hitDirectionDot = Vector3.Dot(hitDirection, surfaceDirection);
            float surfaceDot = Vector3.Dot(a.surfaceNormal, b.surfaceNormal);
            
            return Mathf.Abs(1 - hitDirectionDot) <= SameSurfaceNormalDot &&
                   Mathf.Abs(1 - surfaceDot) <= SameSurfaceNormalDot;
        }

        private static TorchRaycastHit Raycast(Vector3 origin, Vector3 direction, float distance)
        {
            Ray ray = new (origin, direction);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, distance, 1 << LayerIndex.LightObstacle))
            {
                return new TorchRaycastHit
                {
                    surfaceNormal = hitInfo.normal,
                    colliderInstanceId = hitInfo.colliderInstanceID,
                    rayTraveledDistance = hitInfo.distance,
                    rayEndPoint = hitInfo.point
                };
            }

            return new TorchRaycastHit
            {
                rayTraveledDistance = distance,
                rayEndPoint = ray.origin + ray.direction * distance
            };
        }

        private static Vector3 DirectionFromAngle(float angleRad)
        {
            float sin = Mathf.Sin(angleRad);
            float cos = Mathf.Cos(angleRad);
            return new Vector3(sin, 0, cos);
        }
    }
}