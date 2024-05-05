using UnityEngine;
using UnityEngine.Serialization;

namespace Moziborn.Audio
{
    public class AudioConfig : ScriptableObject
    {
        public AudioSourcePrefab audioSourcePrefab;
        public AudioListenerPrefab audioListenerPrefab;
        public AudioEventConfig[] events;
    }
}