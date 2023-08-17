using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerInputController : IUnorderedUpdatable
    {
        public PlayerInputState state { get; private set; }
        
        public void Update()
        {
            PlayerInputState inputState = new PlayerInputState();

            int forward = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            int right = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
            
            if (forward != 0 || right != 0)
            {
                inputState.isMoving = true;
                inputState.forward = forward;
                inputState.right = right;
            }

            state = inputState;
        }
    }
}