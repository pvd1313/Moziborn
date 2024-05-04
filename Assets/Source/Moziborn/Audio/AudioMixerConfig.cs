using System;
using UnityEngine.Audio;

namespace Moziborn.Audio
{
    [Serializable]
    public class AudioMixerConfig
    {
        public AudioMixerId id;
        public AudioMixerGroup audioMixerGroup;
    }
}