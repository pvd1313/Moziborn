using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ProjectTolerance.Rendering.WarFog
{
    [Serializable]
    [VolumeComponentMenuForRenderPipeline("ProjectTolerance/WarFog", typeof(UniversalRenderPipeline))]
    public class WarFogVolume : VolumeComponent, IPostProcessComponent
    {
        public bool IsActive()
        {
            return true;
        }

        public bool IsTileCompatible()
        {
            return true;
        }
    }
}