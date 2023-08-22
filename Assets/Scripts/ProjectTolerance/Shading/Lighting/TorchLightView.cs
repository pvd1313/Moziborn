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
        private const float HitPenetrationSurfaceNormalDistance = 0.03f;
        private const float HitDirectionDotApproximation = 0.001f;
        
        [SerializeField] private MeshFilter _meshFilter;
        
        private float _spreadAngleRad;
        private float _distance;
        private float _rotationRad;
        private int _sampleCount;
        private Vector3 _origin;
        
        private Mesh _mesh;
        private Vector3[] _meshVertices;
        private Vector2[] _meshUVs;
        private int[] _meshTriangles;

        public void Initialize(float spreadAngleRad, float distance)
        {
            _sampleCount = Mathf.Max(2, Mathf.CeilToInt(spreadAngleRad * MaxSamplesPerRad));
            _spreadAngleRad = spreadAngleRad;
            _distance = distance;

            _mesh = new Mesh
            {
                name = "TorchLight",
            };

            _meshVertices = new Vector3[_sampleCount + 1];
            _meshUVs = new Vector2[_sampleCount + 1];
            _meshTriangles = new int[(_sampleCount - 1) * 3];

            for (int i = 0; i < _sampleCount - 1; i++)
            {
                _meshTriangles[i * 3 + 0] = 0;
                _meshTriangles[i * 3 + 1] = i + 1;
                _meshTriangles[i * 3 + 2] = i + 2;
            }

            for (int i = 1; i < _sampleCount + 1; i++)
            {
                _meshUVs[i] = new Vector2(1, 1);
            }

            _mesh.bounds = new Bounds(_origin, Vector3.one * 1000);
            _mesh.SetVertices(_meshVertices);
            _mesh.SetUVs(0, _meshUVs);
            _mesh.SetTriangles(_meshTriangles, 0);
            _mesh.UploadMeshData(false);

            _meshFilter.mesh = _mesh;
        }

        public void UpdatePositionRotation(Vector3 position, float rotationRad)
        {
            _origin = position + Vector3.up * 0.1f;
            _rotationRad = rotationRad;
        }

        public void RebuildMesh()
        {
            float deltaAngleRad = _spreadAngleRad / (_sampleCount - 1);
            float startAngleRad = Mathf.CeilToInt((-0.5f * _spreadAngleRad + _rotationRad) / deltaAngleRad) * deltaAngleRad;

            _meshVertices[0] = _origin;
            
            Vector3 rayDirection = DirectionFromAngle(startAngleRad);
            TorchRaycastHit prevHit = Raycast(_origin, rayDirection, _distance);
            _meshVertices[1] = VertexFromHit(_origin, rayDirection, prevHit.rayTraveledDistance, prevHit.surfaceNormal);
            
            if (DebugDraw) Debug.DrawLine(_origin, _meshVertices[1], Color.red);

            for (int prevHitIndex = 0; prevHitIndex < _sampleCount - 1;)
            {
                int curHitIndex = Mathf.Min( _sampleCount - 1, prevHitIndex + BatchSize);
                int batchSize = curHitIndex - prevHitIndex;
                float angleRad = startAngleRad + deltaAngleRad * curHitIndex;

                rayDirection = DirectionFromAngle(angleRad);
                TorchRaycastHit curHit = Raycast(_origin, rayDirection, _distance);
                _meshVertices[curHitIndex + 1] = VertexFromHit(_origin, rayDirection, curHit.rayTraveledDistance, curHit.surfaceNormal);
                
                if (DebugDraw) Debug.DrawLine(_origin, _meshVertices[curHitIndex], Color.red);

                if (IsSameSurface(prevHit, curHit))
                {
                    for (int i = 0; i < batchSize - 1; i++)
                    {
                        rayDirection = DirectionFromAngle(startAngleRad + deltaAngleRad * (prevHitIndex + i + 1));
                        float distance = Mathf.Lerp(prevHit.rayTraveledDistance, curHit.rayTraveledDistance, (i + 1f) / batchSize);
                        _meshVertices[prevHitIndex + i + 2] = VertexFromHit(_origin, rayDirection, distance, prevHit.surfaceNormal);
                    }
                }
                else
                {
                    for (int i = 0; i < batchSize - 1; i++)
                    {
                        rayDirection = DirectionFromAngle(startAngleRad + deltaAngleRad * (prevHitIndex + i + 1));
                        TorchRaycastHit curBatchHit = Raycast(_origin, rayDirection, _distance);

                        _meshVertices[prevHitIndex + i + 2] = VertexFromHit(_origin, rayDirection, curBatchHit.rayTraveledDistance, curBatchHit.surfaceNormal);
                        
                        if (DebugDraw) Debug.DrawLine(_origin, _meshVertices[prevHitIndex + i + 1], Color.cyan);
                    }
                }

                prevHitIndex = curHitIndex;
                prevHit = curHit;
            }
            
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
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

            Vector3 hitDirection = (b.hitPosition - a.hitPosition).normalized;
            Vector3 surfaceDirection = Vector3.Cross(a.surfaceNormal, Vector3.up);
            float hitDirectionDot = Vector3.Dot(hitDirection, surfaceDirection);
            
            return Mathf.Abs(1 - hitDirectionDot) <= HitDirectionDotApproximation;
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

        private static Vector3 VertexFromHit(Vector3 rayOrigin, Vector3 rayDirection, float rayTraveledDistance, Vector3 surfaceNormal)
        {
            return rayOrigin + rayDirection * rayTraveledDistance - surfaceNormal * HitPenetrationSurfaceNormalDistance;
        }

        private static Vector3 DirectionFromAngle(float angleRad)
        {
            float sin = Mathf.Sin(angleRad);
            float cos = Mathf.Cos(angleRad);
            return new Vector3(sin, 0, cos);
        }
    }
}