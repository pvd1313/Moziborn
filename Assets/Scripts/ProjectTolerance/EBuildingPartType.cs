using System;

namespace ProjectTolerance
{
    [Flags]
    public enum EBuildingPartType
    {
        None = 0,
        NorthWall = 1,
        SouthWall = 2,
        WestWall = 4,
        EastWall = 8,
        Roof = 16,
        All = -1
    }
}