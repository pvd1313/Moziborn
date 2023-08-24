using ProjectTolerance.Layers;
using UnityEngine;

namespace ProjectTolerance.Shading.Lighting
{
    public class TorchLightView : MonoBehaviour
    {
        private struct TorchRaycastHit
        {
            public int colliderInstanceId;
            public float rayTraveledDistance; 
            public Vector3 surfaceNormal;
            public Vector3 hitPosition;
        }

        private const bool DebugDraw = true;
        
        private const int BatchSize = 15;
        private const float MaxSamplesPerRad = 300;
        private const float SameSurfaceNormalDot = 0.0001f;
        
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        
        private float _spreadAngleRad;
        private float _torchDistance;
        private float _torchRotationRad;
        private float _sampleDeltaAngleRad;
        private int _maxSampleCount;
        private Vector3 _torchCenter;
        private Transform _torchTransform;
        
        private Mesh _mesh;
        private Vector3[] _meshVertices;
        private Vector2[] _meshUVs;
        private Color32[] _meshColors;
        private int[] _meshTriangles;

        public void Initialize(float spreadAngleRad, float distance)
        {
            _torchTransform = transform;
            _maxSampleCount = Mathf.Max(2, Mathf.CeilToInt(spreadAngleRad * MaxSamplesPerRad));
            _spreadAngleRad = spreadAngleRad;
            _sampleDeltaAngleRad = _spreadAngleRad / (_maxSampleCount - 1);
            _torchDistance = distance;

            _mesh = new Mesh
            {
                name = "TorchLight",
            };

            _meshVertices = new Vector3[_maxSampleCount + 1];
            _meshUVs = new Vector2[_maxSampleCount + 1];
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

            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * distance);
            _mesh.SetVertices(_meshVertices);
            _mesh.SetColors(_meshColors);
            _mesh.SetUVs(0, _meshUVs);
            _mesh.SetTriangles(_meshTriangles, 0);
            _mesh.UploadMeshData(false);
            
            _meshFilter.mesh = _mesh;

            MaterialPropertyBlock propertyBlock = new ();
            propertyBlock.SetFloat("_MaxRayDistance", distance);
            propertyBlock.SetFloat("_SpreadAngleRad", spreadAngleRad);

            _meshRenderer.SetPropertyBlock(propertyBlock);
        }

        public void UpdatePositionRotation(Vector3 position, float rotationRad)
        {
            _torchCenter = position + Vector3.up * 0.1f;
            _torchRotationRad = rotationRad;
        }

        public void RebuildMesh()
        {
            float torchRotationRad = Mathf.CeilToInt(_torchRotationRad / _sampleDeltaAngleRad) * _sampleDeltaAngleRad;
            float startAngleRad = -0.5f * _spreadAngleRad + torchRotationRad;

            _torchTransform.forward = DirectionFromAngle(torchRotationRad);
            _torchTransform.position = _torchCenter;

            _meshUVs[0] = Vector2.zero;
            
            Vector3 rayDirection = DirectionFromAngle(startAngleRad);
            TorchRaycastHit prevHit = Raycast(_torchCenter, rayDirection, _torchDistance);
            _meshUVs[1] = new Vector2(prevHit.rayTraveledDistance / _torchDistance, 0);

            if (DebugDraw) Debug.DrawRay(_torchCenter, rayDirection * prevHit.rayTraveledDistance, Color.red);

            for (int prevHitIndex = 0; prevHitIndex < _maxSampleCount - 1;)
            {
                int curHitIndex = Mathf.Min( _maxSampleCount - 1, prevHitIndex + BatchSize);
                int batchSize = curHitIndex - prevHitIndex;
                float angleRad = startAngleRad + _sampleDeltaAngleRad * curHitIndex;

                rayDirection = DirectionFromAngle(angleRad);
                TorchRaycastHit curHit = Raycast(_torchCenter, rayDirection, _torchDistance);
                _meshUVs[curHitIndex + 1] = new Vector2(curHit.rayTraveledDistance / _torchDistance, 0);

                if (DebugDraw) Debug.DrawRay(_torchCenter, rayDirection * curHit.rayTraveledDistance, Color.red);

                if (IsSameSurface(prevHit, curHit))
                {
                    if (prevHit.colliderInstanceId != 0)
                    {
                        Plane surface = new Plane(prevHit.surfaceNormal, prevHit.hitPosition);
                    
                        for (int i = 0; i < batchSize - 1; i++)
                        {
                            angleRad = startAngleRad + _sampleDeltaAngleRad * (prevHitIndex + i + 1);
                            rayDirection = DirectionFromAngle(angleRad);
                            Ray ray = new (_torchCenter, rayDirection);
                            surface.Raycast(ray, out float distance);
                            _meshUVs[prevHitIndex + i + 2] = new Vector2(distance / _torchDistance, 0);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < batchSize - 1; i++)
                        {
                            _meshUVs[prevHitIndex + i + 2] = new Vector2(1, 0);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < batchSize - 1; i++)
                    {
                        angleRad = startAngleRad + _sampleDeltaAngleRad * (prevHitIndex + i + 1);
                        rayDirection = DirectionFromAngle(angleRad);
                        TorchRaycastHit curBatchHit = Raycast(_torchCenter, rayDirection, _torchDistance);
                        _meshUVs[prevHitIndex + i + 2] = new Vector2(curBatchHit.rayTraveledDistance / _torchDistance, 0);

                        if (DebugDraw) Debug.DrawRay(_torchCenter, rayDirection * curBatchHit.rayTraveledDistance, Color.cyan);
                    }
                }

                prevHitIndex = curHitIndex;
                prevHit = curHit;
            }
            
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
            _mesh.SetUVs(0, _meshUVs);
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
            
            Vector3 hitDirection = (b.hitPosition - a.hitPosition).normalized;
            Vector3 surfaceDirection = Vector3.Cross(a.surfaceNormal, Vector3.up);
            float hitDirectionDot = Vector3.Dot(hitDirection, surfaceDirection);
            float surfaceDot = Vector3.Dot(a.surfaceNormal, b.surfaceNormal);
            
            return Mathf.Abs(1 - hitDirectionDot) <= SameSurfaceNormalDot &&
                   Mathf.Abs(1 - surfaceDot) <= SameSurfaceNormalDot;
        }

        private static TorchRaycastHit Raycast(Vector3 origin, Vector3 direction, float distance)
        {
            Ray ray = new (origin, direction);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, distance, 1 << LayerId.ViewObstacle))
            {
                return new TorchRaycastHit
                {
                    surfaceNormal = hitInfo.normal,
                    colliderInstanceId = hitInfo.colliderInstanceID,
                    rayTraveledDistance = hitInfo.distance,
                    hitPosition = hitInfo.point
                };
            }

            return new TorchRaycastHit
            {
                rayTraveledDistance = distance,
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