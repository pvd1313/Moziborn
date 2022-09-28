using System;

namespace ProjectTolerance
{
    [Flags]
    public enum EBuildingPartLocation
    {
        None = 0,
        North = 1,
        South = 2,
        West = 4,
        East = 8,
        Roof = 16,
        Floor = 32,
        All = -1
    }
}