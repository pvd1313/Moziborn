using ProjectTolerance.Cinematics;
using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerInputController : IUnorderedUpdatable
    {
        public PlayerInputState state { get; private set; }

        private readonly CameraController _cameraController;
        
        public PlayerInputController (CameraController cameraController)
        {
            _cameraController = cameraController;
        }
        
        public void Update()
        {
            PlayerInputState inputState = new ();

            int forward = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            int right = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
            
            if (forward != 0 || right != 0)
            {
                inputState.isMoving = true;
                inputState.motionForward = forward;
                inputState.motionRight = right;
            }

            Ray cameraCenterRay = _cameraController.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
            Ray mouseRay = _cameraController.ScreenPointToRay(Input.mousePosition);
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