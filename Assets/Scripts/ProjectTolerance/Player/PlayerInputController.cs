using ProjectTolerance.Cinematics;
using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerInputController : IUnorderedUpdatable
    {
        public PlayerInputState state { get; private set; }

        private readonly MainCameraController _mainCamera;

        private bool _inputIsFrozen;
        
        public PlayerInputController (MainCameraController mainCamera)
        {
            _mainCamera = mainCamera;
        }
        
        public void Update()
        {
            PlayerInputState inputState = new ();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _inputIsFrozen = !_inputIsFrozen;
            }

            if (_inputIsFrozen)
            {
                return;
            }

            int forward = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            int right = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;

            inputState.isDropFlareKeyPressed = Input.GetKeyDown(KeyCode.E);

            inputState.isRemoveAllFlaresKeyPressed = Input.GetKeyDown(KeyCode.Q);

            if (forward != 0 || right != 0)
            {
                inputState.isMoving = true;
                inputState.motionForward = forward;
                inputState.motionRight = right;
            }

            Ray cameraCenterRay = _mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
            Ray mouseRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane worldPlane = new Plane(Vector3.up, 0);

            if (worldPlane.Raycast(cameraCenterRay, out float cameraCenterRayLength) &&
                worldPlane.Raycast(mouseRay, out float mouseRayLength))
            {
                Vector3 cameraCenter = cameraCenterRay.origin + cameraCenterRay.direction * cameraCenterRayLength;
                Vector3 mousePosition = mouseRay.origin + mouseRay.direction * mouseRayLength;
                Vector3 viewDirection = (mousePosition - cameraCenter).normalized;
                inputState.aimDirectionAngleRad = Mathf.Atan2(viewDirection.x, viewDirection.z);
                
                Debug.DrawRay(mousePosition - Vector3.forward * 0.5f, Vector3.forward, Color.gray);
                Debug.DrawRay(mousePosition - Vector3.right * 0.5f, Vector3.right, Color.gray);
            }

            state = inputState;
        }
    }
}