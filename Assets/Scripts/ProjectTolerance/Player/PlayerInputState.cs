namespace ProjectTolerance.Player
{
    public struct PlayerInputState
    {
        public float motionForward;
        public float motionRight;
        public float aimDirectionAngleRad;

        public bool isDropFlareKeyPressed;
        public bool isRemoveAllFlaresKeyPressed;
        public bool isMoving;
    }
}