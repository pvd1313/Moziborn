using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Moziborn.Audio
{
    [Serializable]
    public class AudioEventConfig
    {
        public string id;
        public AudioClip[] audioClips;
        public AudioMixerGroup audioMixerGroup;
        public bool loop;
        public float minDistance;
        public float maxDistance;
        public float minPith;
        public float maxPitch;
        public float volumeMin;
        public float volumeMax;
    }
}