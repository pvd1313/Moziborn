using Moziborn.Cinematics;
using UnityEngine;

namespace Moziborn.Player
{
    public class PlayerInputController
    {
        public PlayerInputState state { get; private set; }

        private readonly MainCameraController _mainCamera;

        private bool _inputIsFrozen;

        public PlayerInputController(MainCameraController mainCamera)
        {
            _mainCamera = mainCamera;
        }

        public void Update()
        {
            PlayerInputState inputState = default;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _inputIsFrozen = !_inputIsFrozen;
            }

            if (_inputIsFrozen)
            {
                return;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                inputState.mode = PlayerInputMode.Fire;
            }

            int forwardThrust = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            int sideThrust = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
            if (inputState.mode == PlayerInputMode.Invalid && (forwardThrust != 0 || sideThrust != 0))
            {
                Vector3 forward = Vector3.ProjectOnPlane(_mainCamera.forward, Vector3.up).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                inputState.movementDirection = (forward * forwardThrust + right * sideThrust).normalized;
                inputState.mode = PlayerInputMode.Movement;
            }
            else
            {
                inputState.movementDirection = Vector3.zero;
            }

            Ray cameraCenterRay =
                _mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
            Ray mouseRay = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane worldPlane = new(Vector3.up, 0);

            if (worldPlane.Raycast(cameraCenterRay, out float cameraCenterRayLength) &&
                worldPlane.Raycast(mouseRay, out float mouseRayLength))
            {
                Vector3 cameraCenter = cameraCenterRay.origin + cameraCenterRay.direction * cameraCenterRayLength;
                Vector3 mousePosition = mouseRay.origin + mouseRay.direction * mouseRayLength;
                inputState.aimDirection = (mousePosition - cameraCenter).normalized;

                Debug.DrawRay(mousePosition - Vector3.forward * 0.5f, Vector3.forward, Color.gray);
                Debug.DrawRay(mousePosition - Vector3.right * 0.5f, Vector3.right, Color.gray);
            }

            inputState.fireButton = GetMouseButtonState(0);
            inputState.flashlightButton = GetKeyState(KeyCode.F);
            inputState.dropFlareButton = GetKeyState(KeyCode.E);
            inputState.removeAllFlaresButton = GetKeyState(KeyCode.Q);
            
            if (inputState.mode == PlayerInputMode.Invalid)
            {
                inputState.mode = PlayerInputMode.Interact;
            }

            state = inputState;
        }

        private InputButtonState GetMouseButtonState(int buttonId)
        {
            InputButtonState buttonState = InputButtonState.None;
            
            if (Input.GetMouseButton(buttonId))
            {
                buttonState |= InputButtonState.IsHeldDown;
            }
            
            if (Input.GetMouseButtonDown(buttonId))
            {
                buttonState |= InputButtonState.PressedThisFrame;
            }
            
            if (Input.GetMouseButtonUp(buttonId))
            {
                buttonState |= InputButtonState.ReleasedThisFrame;
            }

            return buttonState;
        }

        private InputButtonState GetKeyState(KeyCode keyCode)
        {
            InputButtonState buttonState = InputButtonState.None;

            if (Input.GetKey(keyCode))
            {
                buttonState |= InputButtonState.IsHeldDown;
            }

            if (Input.GetKeyDown(keyCode))
            {
                buttonState |= InputButtonState.PressedThisFrame;
            }

            if (Input.GetKeyUp(keyCode))
            {
                buttonState |= InputButtonState.ReleasedThisFrame;
            }

            return buttonState;
        }
        
    }
}