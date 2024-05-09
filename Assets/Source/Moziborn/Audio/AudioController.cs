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
        private struct AudioPlayerData
        {
            public int eventId;
            public AudioSourcePrefab audioSource;
            public bool isPlaying;
            public IPositionTransform transform;
        }
        
        private readonly AudioConfig _config;

        private Dictionary<string, AudioEventConfig> _events;
        private List<int> _freeAudioPlayers;
        private List<AudioPlayerData> _audioPlayers;
        private Dictionary<int, int> _activePlayersIndexMap;
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
            _activePlayersIndexMap = new Dictionary<int, int>();
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
            int eventId = StartEvent(eventName, transform.position, duration);
            int eventListIndex = _activePlayersIndexMap[eventId];
            AudioPlayerData player = _audioPlayers[eventListIndex];
            player.transform = transform;
            _audioPlayers[eventListIndex] = player;

            return eventId;
        }

        public int StartEvent(string eventName, Vector3 position, float duration)
        {
            if (!_events.TryGetValue(eventName, out AudioEventConfig eventConfig))
            {
                throw new InvalidOperationException($"Unknown audio event '{eventName}'");
            }
            
            int playerListIndex = GetOrCreateAudioPlayer();
            
            AudioPlayerData player = _audioPlayers[playerListIndex];
            player.audioSource.transform.position = position;
            player.audioSource.audioSource.minDistance = eventConfig.minDistance;
            player.audioSource.audioSource.maxDistance = eventConfig.maxDistance;
            player.audioSource.audioSource.loop = false;
            player.audioSource.audioSource.volume = Random.Range(eventConfig.volumeMin, eventConfig.volumeMax);
            int randomClipIndex = Random.Range(0, eventConfig.audioClips.Length);
            player.audioSource.audioSource.clip = eventConfig.audioClips[randomClipIndex];
            player.audioSource.audioSource.outputAudioMixerGroup = eventConfig.audioMixerGroup;
            player.audioSource.audioSource.pitch = eventConfig.audioClips[randomClipIndex].length / duration;
            player.audioSource.audioSource.Play();
            
            player.eventId = _nextEventId;
            player.transform = null;
            player.isPlaying = true;
            _audioPlayers[playerListIndex] = player;
            
            _activePlayersIndexMap[_nextEventId] = playerListIndex;

            return _nextEventId++;
        }
        
        public int StartEvent(string eventName, IPositionTransform transform)
        {
            int eventId = StartEvent(eventName, transform.position);
            int eventListIndex = _activePlayersIndexMap[eventId];
            AudioPlayerData player = _audioPlayers[eventListIndex];
            player.transform = transform;
            _audioPlayers[eventListIndex] = player;

            return eventId;
        }

        public int StartEvent(string eventName, Vector3 position)
        {
            if (!_events.TryGetValue(eventName, out AudioEventConfig eventConfig))
            {
                throw new InvalidOperationException($"Unknown audio event '{eventName}'");
            }

            int playerListIndex = GetOrCreateAudioPlayer();

            AudioPlayerData player = _audioPlayers[playerListIndex];
            player.audioSource.transform.position = position;
            player.audioSource.audioSource.minDistance = eventConfig.minDistance;
            player.audioSource.audioSource.maxDistance = eventConfig.maxDistance;
            player.audioSource.audioSource.loop = eventConfig.loop;
            player.audioSource.audioSource.pitch = Random.Range(eventConfig.minPith, eventConfig.maxPitch);
            player.audioSource.audioSource.volume = Random.Range(eventConfig.volumeMin, eventConfig.volumeMax);
            int randomClipIndex = Random.Range(0, eventConfig.audioClips.Length);
            player.audioSource.audioSource.clip = eventConfig.audioClips[randomClipIndex];
            player.audioSource.audioSource.outputAudioMixerGroup = eventConfig.audioMixerGroup;
            player.audioSource.audioSource.Play();

            player.eventId = _nextEventId;
            player.transform = null;
            player.isPlaying = true;
            _audioPlayers[playerListIndex] = player;
            
            _activePlayersIndexMap[_nextEventId] = playerListIndex;

            return _nextEventId++;
        }

        private int GetOrCreateAudioPlayer()
        {
            if (_freeAudioPlayers.Count == 0)
            {
                AudioSourcePrefab audioInstance = GameObject.Instantiate(_config.audioSourcePrefab, _root);
                
                _audioPlayers.Add(new AudioPlayerData
                {
                    eventId = -1,
                    audioSource = audioInstance
                });

                return _audioPlayers.Count - 1;
            }

            int playerIndex = _freeAudioPlayers[^1];
            _freeAudioPlayers.RemoveAt(_freeAudioPlayers.Count - 1);

            return playerIndex;
        }

        public void StopEvent(int eventId)
        {
            if (!_activePlayersIndexMap.TryGetValue(eventId, out int listIndex))
            {
                return;
            }

            AudioPlayerData player = _audioPlayers[listIndex];
            player.audioSource.audioSource.Stop();
            player.isPlaying = false;
            player.eventId = -1;
            _audioPlayers[listIndex] = player;
            _freeAudioPlayers.Add(listIndex);

            _activePlayersIndexMap.Remove(eventId);
        }

        public bool IsPlaying(int eventId)
        {
            return _activePlayersIndexMap.ContainsKey(eventId);
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

                if (player.audioSource.audioSource.isPlaying)
                {
                    if (player.transform != null)
                    {
                        player.audioSource.transform.position = player.transform.position;
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