using System;
using ProjectTolerance.Layers;
using ProjectTolerance.Mathematics;
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
        private const float HitPenetrationDistance = 0.03f;
        private const float SameSurfaceNormalDot = 0.001f;
        
        [SerializeField] private MeshFilter _meshFilter;
        
        private float _spreadAngleRad;
        private float _torchDistance;
        private float _torchRotationRad;
        private float _sampleDeltaAngleRad;
        private int _maxSampleCount;
        private Vector3 _torchCenter;
        
        private Mesh _mesh;
        private Vector3[] _meshVertices;
        private Vector2[] _meshUVs;
        private Color32[] _meshColors;
        private int[] _meshTriangles;

        public void Initialize(float spreadAngleRad, float distance)
        {
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

            _mesh.bounds = new Bounds(_torchCenter, Vector3.one * 1000);
            _mesh.SetVertices(_meshVertices);
            _mesh.SetColors(_meshColors);
            _mesh.SetUVs(0, _meshUVs);
            _mesh.SetTriangles(_meshTriangles, 0);
            _mesh.UploadMeshData(false);
            
            _meshFilter.mesh = _mesh;
        }

        public void UpdatePositionRotation(Vector3 position, float rotationRad)
        {
            _torchCenter = position + Vector3.up * 0.1f;
            _torchRotationRad = rotationRad;
        }

        public void RebuildMesh()
        {
            float startAngleRad = Mathf.CeilToInt((-0.5f * _spreadAngleRad + _torchRotationRad) / _sampleDeltaAngleRad) * _sampleDeltaAngleRad;

            _meshVertices[0] = _torchCenter;
            _meshUVs[0] = Vector2.one * 0.5f;
            _meshColors[0] = default;
            
            Vector3 rayDirection = DirectionFromAngle(startAngleRad);
            TorchRaycastHit prevHit = Raycast(_torchCenter, rayDirection, _torchDistance);
            _meshVertices[1] = VertexFromHit(_torchCenter, rayDirection, prevHit.rayTraveledDistance);
            _meshUVs[1] = UVFromDirectionDistance(rayDirection, prevHit.rayTraveledDistance);
            _meshColors[1] = ColorFromIndexDistance(0, prevHit.rayTraveledDistance);

            if (DebugDraw) Debug.DrawLine(_torchCenter, _meshVertices[1], Color.red);

            for (int prevHitIndex = 0; prevHitIndex < _maxSampleCount - 1;)
            {
                int curHitIndex = Mathf.Min( _maxSampleCount - 1, prevHitIndex + BatchSize);
                int batchSize = curHitIndex - prevHitIndex;
                float angleRad = startAngleRad + _sampleDeltaAngleRad * curHitIndex;

                rayDirection = DirectionFromAngle(angleRad);
                TorchRaycastHit curHit = Raycast(_torchCenter, rayDirection, _torchDistance);
                _meshVertices[curHitIndex + 1] = VertexFromHit(_torchCenter, rayDirection, curHit.rayTraveledDistance);
                _meshUVs[curHitIndex + 1] = UVFromDirectionDistance(rayDirection, curHit.rayTraveledDistance);
                _meshColors[curHitIndex + 1] = ColorFromIndexDistance(curHitIndex, curHit.rayTraveledDistance);

                if (DebugDraw) Debug.DrawLine(_torchCenter, _meshVertices[curHitIndex], Color.red);

                if (IsSameSurface(prevHit, curHit))
                {
                    for (int i = 0; i < batchSize - 1; i++)
                    {
                        angleRad = startAngleRad + _sampleDeltaAngleRad * (prevHitIndex + i + 1);
                        rayDirection = DirectionFromAngle(angleRad);
                        float distance = Mathf.Lerp(prevHit.rayTraveledDistance, curHit.rayTraveledDistance, (i + 1f) / batchSize);
    
                        _meshVertices[prevHitIndex + i + 2] = VertexFromHit(_torchCenter, rayDirection, distance);
                        _meshUVs[prevHitIndex + i + 2] = UVFromDirectionDistance(rayDirection, distance);
                        _meshColors[prevHitIndex + i + 2] = ColorFromIndexDistance(prevHitIndex + i + 1, distance);
                    }
                }
                else
                {
                    for (int i = 0; i < batchSize - 1; i++)
                    {
                        angleRad = startAngleRad + _sampleDeltaAngleRad * (prevHitIndex + i + 1);
                        rayDirection = DirectionFromAngle(angleRad);
                        TorchRaycastHit curBatchHit = Raycast(_torchCenter, rayDirection, _torchDistance);
                        
                        _meshVertices[prevHitIndex + i + 2] = VertexFromHit(_torchCenter, rayDirection, curBatchHit.rayTraveledDistance);
                        _meshUVs[prevHitIndex + i + 2] = UVFromDirectionDistance(rayDirection, curBatchHit.rayTraveledDistance);
                        _meshColors[prevHitIndex + i + 2] = ColorFromIndexDistance(prevHitIndex + i + 1, curBatchHit.rayTraveledDistance);

                        if (DebugDraw) Debug.DrawLine(_torchCenter, _meshVertices[prevHitIndex + i + 1], Color.cyan);
                    }
                }

                prevHitIndex = curHitIndex;
                prevHit = curHit;
            }
            
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
            _mesh.SetVertices(_meshVertices);
            _mesh.SetUVs(0, _meshUVs);
            _mesh.SetColors(_meshColors);
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
            
            return Mathf.Abs(1 - hitDirectionDot) <= SameSurfaceNormalDot;
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
                rayTraveledDistance = distance
            };
        }

        private static Vector3 VertexFromHit(Vector3 rayOrigin, Vector3 rayDirection, float rayTraveledDistance)
        {
            return rayOrigin + rayDirection * rayTraveledDistance + rayDirection * HitPenetrationDistance;
        }

        private static Vector3 DirectionFromAngle(float angleRad)
        {
            float sin = Mathf.Sin(angleRad);
            float cos = Mathf.Cos(angleRad);
            return new Vector3(sin, 0, cos);
        }

        private Vector2 UVFromDirectionDistance(Vector3 direction, float distance)
        {
            return (Vector2.one + direction.XZ() * (distance / _torchDistance)) * 0.5f;
        }

        private Color32 ColorFromIndexDistance(int rayIndex, float distance)
        {
            float angleRad = -0.5f * _spreadAngleRad + rayIndex * _sampleDeltaAngleRad;
            float t = 1 - Mathf.Abs(angleRad / (_spreadAngleRad * 0.5f));
            t *= (distance / _torchDistance);
            
            byte color = (byte) (t * 255);

            return new Color32(color, color, color, color);
        }
    }
}