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
        private const float HitPenetrationDistance = 0.03f;
        private const float SameSurfaceNormalDot = 0.001f;
        
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

            _meshVertices[0] = new Vector3(0, 0, -0.00f);
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
            float startAngleRad = Mathf.CeilToInt((-0.5f * _spreadAngleRad + _torchRotationRad) / _sampleDeltaAngleRad) * _sampleDeltaAngleRad;

            _torchTransform.forward = DirectionFromAngle(startAngleRad + _sampleDeltaAngleRad * _maxSampleCount * 0.5f);
            _torchTransform.position = _torchCenter;
            
            _meshColors[0] = Float01ToColor(0);
            
            Vector3 rayDirection = DirectionFromAngle(startAngleRad);
            TorchRaycastHit prevHit = Raycast(_torchCenter, rayDirection, _torchDistance);
            _meshColors[1] = Float01ToColor(prevHit.rayTraveledDistance / _torchDistance);

            if (DebugDraw) Debug.DrawRay(_torchCenter, rayDirection * prevHit.rayTraveledDistance, Color.red);

            for (int prevHitIndex = 0; prevHitIndex < _maxSampleCount - 1;)
            {
                int curHitIndex = Mathf.Min( _maxSampleCount - 1, prevHitIndex + BatchSize);
                int batchSize = curHitIndex - prevHitIndex;
                float angleRad = startAngleRad + _sampleDeltaAngleRad * curHitIndex;

                rayDirection = DirectionFromAngle(angleRad);
                TorchRaycastHit curHit = Raycast(_torchCenter, rayDirection, _torchDistance);
                _meshColors[curHitIndex + 1] = Float01ToColor(curHit.rayTraveledDistance / _torchDistance);

                if (DebugDraw) Debug.DrawRay(_torchCenter, rayDirection * curHit.rayTraveledDistance, Color.red);

                if (IsSameSurface(prevHit, curHit))
                {
                    for (int i = 0; i < batchSize - 1; i++)
                    {
                        float distance = Mathf.Lerp(prevHit.rayTraveledDistance, curHit.rayTraveledDistance, (i + 1f) / batchSize);
                        _meshColors[prevHitIndex + i + 2] = Float01ToColor(distance / _torchDistance);
                    }
                }
                else
                {
                    for (int i = 0; i < batchSize - 1; i++)
                    {
                        angleRad = startAngleRad + _sampleDeltaAngleRad * (prevHitIndex + i + 1);
                        rayDirection = DirectionFromAngle(angleRad);
                        TorchRaycastHit curBatchHit = Raycast(_torchCenter, rayDirection, _torchDistance);
                        _meshColors[prevHitIndex + i + 2] = Float01ToColor(curBatchHit.rayTraveledDistance / _torchDistance);

                        if (DebugDraw) Debug.DrawRay(_torchCenter, rayDirection * curBatchHit.rayTraveledDistance, Color.cyan);
                    }
                }

                prevHitIndex = curHitIndex;
                prevHit = curHit;
            }
            
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
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

        private static Vector3 DirectionFromAngle(float angleRad)
        {
            float sin = Mathf.Sin(angleRad);
            float cos = Mathf.Cos(angleRad);
            return new Vector3(sin, 0, cos);
        }
        
        private static Color32 Float01ToColor(float float01)
        {
            ushort word = (ushort) (float01 * ushort.MaxValue);
            return new Color32((byte)(word / 256), (byte)(word % 256), 0 , 0);
        }
    }
}