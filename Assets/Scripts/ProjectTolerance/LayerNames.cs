using UnityEngine;

namespace ProjectTolerance
{
    public static class LayerNames
    {
        public static readonly int Default = LayerMask.NameToLayer("Default");
        public static readonly int Interior = LayerMask.NameToLayer("Interior");
        public static readonly int Door = LayerMask.NameToLayer("Door");
    }
}