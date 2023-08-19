using ProjectTolerance.Layers;
using UnityEngine;

namespace ProjectTolerance.Shading.Lighting
{
    public class TorchLightView : MonoBehaviour
    {
        private float _spreadAngleDegrees;
        private float _distance;
        private float _sampleCount;
        private float _rotationRad;

        public void Initialize(float spreadAngleDegrees, float distance, int sampleCount)
        {
            _spreadAngleDegrees = spreadAngleDegrees;
            _distance = distance;
            _sampleCount = sampleCount;
        }

        public void UpdatePositionRotation(Vector3 position, float rotationRad)
        {
            transform.position = position;
            _rotationRad = rotationRad;
        }

        public void DebugDrawRaycasts()
        {
            Vector3 center = transform.position;
            float startAngleRad = -0.5f * _spreadAngleDegrees * Mathf.Deg2Rad + _rotationRad;
            float deltaAngle = _spreadAngleDegrees / (_sampleCount - 1) * Mathf.Deg2Rad;

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