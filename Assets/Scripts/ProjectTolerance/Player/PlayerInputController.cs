using ProjectTolerance.GameCycle;
using UnityEngine;

namespace ProjectTolerance.Player
{
    public class PlayerInputController : IUnorderedUpdatable
    {
        public PlayerInputState State { get; private set; }
        
        public void Update()
        {
            var inputState = new PlayerInputState();

            var forward = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            var side = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
            if (forward != 0 || side != 0)
            {
                inputState.closeAim = true;
                inputState.closeAimDirection = new Vector2(forward, side).normalized;
            }

            if (Input.GetMouseButton(1))
            {
                inputState.movement = true;
            }

            State = inputState;
        }
    }
}