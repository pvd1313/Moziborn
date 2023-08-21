using ProjectTolerance.Layers;
using UnityEngine;

namespace ProjectTolerance.Shading.Lighting
{
    public class TorchLightView : MonoBehaviour
    {
        private const float SamplesPerRad = 500;
        private const float HitPenetrationDistance = 0.05f;
        
        [SerializeField] private MeshFilter _meshFilter;
        
        private float _spreadAngleRad;
        private float _distance;
        private float _rotationRad;
        private int _sampleCount;
        private Vector3 _position;
        
        private Mesh _mesh;
        private Vector3[] _meshVertices;
        private Vector2[] _meshUVs;
        private int[] _meshTriangles;

        public void Initialize(float spreadAngleRad, float distance)
        {
            _sampleCount = Mathf.Max(2, Mathf.CeilToInt(spreadAngleRad * SamplesPerRad));
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

            _mesh.bounds = new Bounds(_position, Vector3.one * 1000);
            _mesh.SetVertices(_meshVertices);
            _mesh.SetUVs(0, _meshUVs);
            _mesh.SetTriangles(_meshTriangles, 0);
            _mesh.UploadMeshData(false);

            _meshFilter.mesh = _mesh;
        }

        public void UpdatePositionRotation(Vector3 position, float rotationRad)
        {
            _position = position;
            _rotationRad = rotationRad;
        }

        public void RebuildMesh(bool debugDraw)
        {
            Vector3 center = _position;
            float deltaAngle = _spreadAngleRad / (_sampleCount - 1);
            float startAngleRad = Mathf.CeilToInt((-0.5f * _spreadAngleRad + _rotationRad) / deltaAngle) * deltaAngle;

            _meshVertices[0] = center;
            
            for (int i = 0; i < _sampleCount; i++)
            {
                float angle = startAngleRad + deltaAngle * i;
                float sin = Mathf.Sin(angle);
                float cos = Mathf.Cos(angle);

                Vector3 direction = new Vector3(sin, 0, cos) * _distance;

                Ray ray = new Ray(center, direction);

                Vector3 vertexPos;
                
                if (Physics.Raycast(ray, out RaycastHit hitInfo, _distance, 1 << LayerId.ViewObstacle))
                {
                    vertexPos = center + new Vector3(sin, 0, cos) * hitInfo.distance;
                    vertexPos -= hitInfo.normal * HitPenetrationDistance;
                }
                else
                {
                    vertexPos = center + new Vector3(sin, 0, cos) * _distance;
                }

                _meshVertices[i + 1] = vertexPos;
                
                if (debugDraw)
                {
                    Debug.DrawRay(center, vertexPos - center, Color.blue);
                }
            }
            
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
            _mesh.SetVertices(_meshVertices);
            _mesh.UploadMeshData(false);
        }
    }
}