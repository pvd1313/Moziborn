using UnityEngine;

namespace Moziborn.Characters
{
    public class CharacterView
    {
        private readonly CharacterPrefab _character;
        private readonly CharacterMeshPrefab _mesh;

        public Vector3 rigidbodyVelocity
        {
            get => _character.rigidbody.velocity;
            set => _character.rigidbody.velocity = value;
        }

        public Vector3 rigidbodyPosition => _character.rigidbody.position;
        public Vector3 meshPosition => _character.meshRoot.position;
        public Quaternion meshRotation => _character.meshRoot.rotation;
        public Bounds viewBlockColliderBounds => _character.viewBlockCollider.bounds;

        private Vector3 _meshPosition;
        private Vector3 _meshVelocity;

        public CharacterView(CharacterPrefab character, CharacterMeshPrefab mesh)
        {
            _character = character;
            _mesh = mesh;

            _mesh.transform.SetParent(_character.animator.transform);
            
            _character.physicsCollider.radius = _mesh.radius;
            _character.physicsCollider.height = _mesh.height;
            _character.physicsCollider.center = new Vector3(0, _mesh.height * 0.5f, 0);
        }

        public void InterpolateMeshPosition(float smoothTime)
        {
            _meshPosition = Vector3.SmoothDamp(
                _meshPosition,
                _character.rigidbody.position,
                ref _meshVelocity,
                smoothTime);

            _character.meshRoot.position = _meshPosition;
        }

        public void RotateMeshTowards(Vector3 viewDirection, float rotationSpeed, float smoothTime)
        {
            Quaternion aimRotation = Quaternion.LookRotation(viewDirection);
            float aimDeltaAngle = Quaternion.Angle(aimRotation, _character.meshRoot.rotation);
            float aimDeltaTime = aimDeltaAngle / rotationSpeed;
            float speedMultiplier = Mathf.Clamp01(aimDeltaTime / smoothTime);
            
            _character.meshRoot.rotation = Quaternion.RotateTowards(
                _character.meshRoot.rotation, 
                aimRotation,
                Time.deltaTime * rotationSpeed * speedMultiplier);
        }

        public void AttachTransformToMeshRoot(Transform transform)
        {
            transform.SetParent(_character.meshRoot);
        }

        public void AnimatorSetFloat(int propertyId, float value)
        {
            _character.animator.SetFloat(propertyId, value);
        }
    }
}