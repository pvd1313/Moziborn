using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moziborn.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Moziborn.Audio
{
    public class AudioController
    {
        private class AudioPlayerData
        {
            public int eventId;
            public AudioSource audioSource;
            public bool isPlaying;
            public IPositionTransform transform;
            public bool isFadingOut;
            public float initialVolume;
            public float fadeOut01;
            public AudioEventConfig config;
        }
        
        private readonly AudioConfig _config;

        private Dictionary<string, AudioEventConfig> _events;
        private List<int> _freeAudioPlayers;
        private List<AudioPlayerData> _audioPlayers;
        private Dictionary<int, int> _playerEventMap;
        private int _nextEventId;
        private AudioListenerPrefab _audioListener;
        private IPositionTransform _audioListenerTarget;
        private Transform _root;

        public AudioController(AudioConfig config)
        {
            _config = config;
        }

        public IEnumerator Initialize()
        {
            _events = _config.events.ToDictionary(e => e.id);
            _freeAudioPlayers = new List<int>();
            _audioPlayers = new List<AudioPlayerData>();
            _playerEventMap = new Dictionary<int, int>();
            _root = new GameObject("AudioController").transform;
            _audioListener = GameObject.Instantiate(_config.audioListenerPrefab, _root);

            yield break;
        }

        public void SetListener(IPositionTransform transform)
        {
            _audioListenerTarget = transform;
        }

        public int StartEvent(string eventName, IPositionTransform transform, float duration)
        {
            if (!_events.TryGetValue(eventName, out AudioEventConfig eventConfig))
            {
                throw new InvalidOperationException($"Unknown audio event '{eventName}'");
            }
            
            int playerListIndex = GetOrCreateAudioPlayer();
            
            AudioPlayerData player = _audioPlayers[playerListIndex];

            SetupPlayer(player, eventConfig);
            
            player.audioSource.transform.position = transform.position;
            player.transform = transform;
            player.audioSource.pitch = player.audioSource.clip.length / duration;
            
            _playerEventMap[_nextEventId] = playerListIndex;

            return _nextEventId++;
        }

        public int StartEvent(string eventName, Vector3 position, float duration)
        {
            if (!_events.TryGetValue(eventName, out AudioEventConfig eventConfig))
            {
                throw new InvalidOperationException($"Unknown audio event '{eventName}'");
            }
            
            int playerListIndex = GetOrCreateAudioPlayer();
            
            AudioPlayerData player = _audioPlayers[playerListIndex];

            SetupPlayer(player, eventConfig);
            
            player.audioSource.transform.position = position;
            player.audioSource.pitch = player.audioSource.clip.length / duration;
            
            _playerEventMap[_nextEventId] = playerListIndex;

            return _nextEventId++;
        }
        
        public int StartEvent(string eventName, IPositionTransform transform)
        {
            if (!_events.TryGetValue(eventName, out AudioEventConfig eventConfig))
            {
                throw new InvalidOperationException($"Unknown audio event '{eventName}'");
            }

            int playerListIndex = GetOrCreateAudioPlayer();

            AudioPlayerData player = _audioPlayers[playerListIndex];
            
            SetupPlayer(player, eventConfig);

            player.audioSource.transform.position = transform.position;
            player.transform = transform;
            
            _playerEventMap[_nextEventId] = playerListIndex;

            return _nextEventId++;
        }

        public int StartEvent(string eventName, Vector3 position)
        {
            if (!_events.TryGetValue(eventName, out AudioEventConfig eventConfig))
            {
                throw new InvalidOperationException($"Unknown audio event '{eventName}'");
            }

            int playerListIndex = GetOrCreateAudioPlayer();

            AudioPlayerData player = _audioPlayers[playerListIndex];
            
            SetupPlayer(player, eventConfig);

            player.audioSource.transform.position = position;
            
            _playerEventMap[_nextEventId] = playerListIndex;

            return _nextEventId++;
        }

        private void SetupPlayer(AudioPlayerData player, AudioEventConfig eventConfig)
        {
            player.audioSource.minDistance = eventConfig.minDistance;
            player.audioSource.maxDistance = eventConfig.maxDistance;
            player.audioSource.loop = eventConfig.loop;
            player.audioSource.pitch = Random.Range(eventConfig.minPith, eventConfig.maxPitch);
            player.audioSource.volume = Random.Range(eventConfig.volumeMin, eventConfig.volumeMax);
            int randomClipIndex = Random.Range(0, eventConfig.audioClips.Length);
            player.audioSource.clip = eventConfig.audioClips[randomClipIndex];
            player.audioSource.outputAudioMixerGroup = eventConfig.audioMixerGroup;
            player.audioSource.Play();

            player.config = eventConfig;
            player.initialVolume = player.audioSource.volume;
            player.eventId = _nextEventId;
            player.transform = null;
            player.isFadingOut = false;
            player.fadeOut01 = 0;
            player.isPlaying = true;
        }

        private int GetOrCreateAudioPlayer()
        {
            if (_freeAudioPlayers.Count == 0)
            {
                AudioSourcePrefab audioInstance = GameObject.Instantiate(_config.audioSourcePrefab, _root);
                
                _audioPlayers.Add(new AudioPlayerData
                {
                    eventId = -1,
                    audioSource = audioInstance.audioSource
                });

                return _audioPlayers.Count - 1;
            }

            int playerIndex = _freeAudioPlayers[^1];
            _freeAudioPlayers.RemoveAt(_freeAudioPlayers.Count - 1);

            return playerIndex;
        }

        public void StopEvent(int eventId)
        {
            if (!_playerEventMap.TryGetValue(eventId, out int listIndex))
            {
                return;
            }

            AudioPlayerData player = _audioPlayers[listIndex];

            if (player.isFadingOut && 
                player.fadeOut01 < 1 && 
                player.audioSource.isPlaying)
            {
                return;
            }

            if (!player.isFadingOut && 
                player.config.fadeOutDuration > 0 && 
                player.audioSource.isPlaying)
            {
                player.isFadingOut = true;
                return;
            }
            
            player.audioSource.Stop();
            player.isPlaying = false;
            player.eventId = -1;
            _freeAudioPlayers.Add(listIndex);

            _playerEventMap.Remove(eventId);
        }

        public bool IsPlaying(int eventId)
        {
            return _playerEventMap.ContainsKey(eventId);
        }

        public void Update()
        {
            for (int i = 0; i < _audioPlayers.Count; i++)
            {
                AudioPlayerData player = _audioPlayers[i];
                if (!player.isPlaying)
                {
                    continue;
                }

                if (player.audioSource.isPlaying)
                {
                    if (player.transform != null)
                    {
                        player.audioSource.transform.position = player.transform.position;
                    }
                    
                    if (player.config.fadeOutDuration > 0)
                    {
                        float timeLeft = player.audioSource.clip.length -
                                         player.audioSource.time;

                        if (timeLeft < player.config.fadeOutDuration)
                        {
                            player.isFadingOut = true;
                        }
                    }

                    if (player.isFadingOut)
                    {
                        float fadeDelta = Time.deltaTime / player.config.fadeOutDuration;
                        player.fadeOut01 = Mathf.Clamp01(player.fadeOut01 + fadeDelta);
                        player.audioSource.volume = player.initialVolume * (1 - player.fadeOut01);

                        if (player.fadeOut01 == 1)
                        {
                            StopEvent(player.eventId);
                        }
                    }
                }
                else
                {
                    StopEvent(player.eventId);
                }
            }

            if (_audioListenerTarget != null)
            {
                _audioListener.transform.position = _audioListenerTarget.position;
            }
        }
    }
}