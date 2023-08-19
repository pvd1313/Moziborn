using ProjectTolerance.Layers;
using UnityEngine;

namespace ProjectTolerance.Shading.Lighting
{
    public class TorchLightView : MonoBehaviour
    {
        private const float SamplesPerRad = 20;
        
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

        public void RebuildMesh()
        {
            Vector3 center = _position;
            float startAngleRad = -0.5f * _spreadAngleRad+ _rotationRad;
            float deltaAngle = _spreadAngleRad / (_sampleCount - 1);

            _meshVertices[0] = center;
            
            for (int i = 0; i < _sampleCount; i++)
            {
                float angle = startAngleRad + deltaAngle * i;
                float sin = Mathf.Sin(angle);
                float cos = Mathf.Cos(angle);

                Vector3 direction = new Vector3(sin, 0, cos) * _distance;

                Ray ray = new Ray(center, direction);
                
                if (Physics.Raycast(ray, out RaycastHit hitInfo, _distance, 1 << LayerId.ViewObstacle))
                {
                    _meshVertices[i + 1] = center + new Vector3(sin, 0, cos) * hitInfo.distance;
                }
                else
                {
                    _meshVertices[i + 1] = center + new Vector3(sin, 0, cos) * _distance;
                }
            }
            
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
            _mesh.SetVertices(_meshVertices);
            _mesh.UploadMeshData(false);
        }

        public void DebugDrawRaycasts()
        {
            Vector3 center = _position;
            float startAngleRad = -0.5f * _spreadAngleRad + _rotationRad;
            float deltaAngle = _spreadAngleRad / (_sampleCount - 1);

            for (int i = 0; i < _sampleCount; i++)
            {
                float angle = startAngleRad + deltaAngle * i;
                float sin = Mathf.Sin(angle);
                float cos = Mathf.Cos(angle);

                Vector3 direction = new Vector3(sin, 0, cos) * _distance;

                Ray ray = new Ray(center, direction);
                
                if (Physics.Raycast(ray, out RaycastHit hitInfo, _distance, 1 << LayerId.ViewObstacle))
                {
                    Debug.DrawRay(center, new Vector3(sin, 0, cos) * hitInfo.distance, Color.blue);
                }
                else
                {
                    Debug.DrawRay(center, new Vector3(sin, 0, cos) * _distance, Color.blue);
                }
            }
        }
    }
}