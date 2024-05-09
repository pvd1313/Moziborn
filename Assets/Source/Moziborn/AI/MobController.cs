using System.Collections;
using System.Collections.Generic;
using Moziborn.Audio;
using Moziborn.Buildings;
using Moziborn.Characters;
using Moziborn.Cinematics;
using Moziborn.Damage;
using Moziborn.Player;
using UnityEngine;

namespace Moziborn.AI
{
    public class MobController
    {
        private readonly MobControllerConfig _config;
        private readonly BuildingController _buildingController;
        private readonly CharacterFactory _characterFactory;
        private readonly DamageController _damageController;
        private readonly PlayerController _playerController;
        private readonly MainCameraController _cameraController;
        private readonly AudioController _audioController;

        private Transform _root;
        private NavigationSurfacePrefab _navigationSurface;

        private List<MobData> _aliveMobs;

        public MobController(MobControllerConfig config, 
                             BuildingController buildingController,
                             CharacterFactory characterFactory,
                             DamageController damageController,
                             PlayerController playerController,
                             MainCameraController cameraController,
                             AudioController audioController)
        {
            _config = config;
            _buildingController = buildingController;
            _characterFactory = characterFactory;
            _damageController = damageController;
            _playerController = playerController;
            _cameraController = cameraController;
            _audioController = audioController;
        }

        public IEnumerator Initialize()
        {
            _root = new GameObject("MobController").transform;

            _navigationSurface = GameObject.Instantiate(_config.navigationSurfacePrefab, _root);
            _navigationSurface.navMeshSurface.BuildNavMesh();

            _aliveMobs = new List<MobData>();

            foreach (Vector2Int pos in _buildingController.enemySpawnPoints)
            {
                Vector3 position = new (pos.x + 0.5f, 0, pos.y + 0.5f);
                SpawnMob(position);
            }
            
            yield break;
        }

        public void Update()
        {
            foreach (MobData mob in _aliveMobs)
            {
                MobLogicUpdate(mob);
            }
        }

        private void MobLogicUpdate(MobData mob)
        {
            Vector3 viewPosition = mob.view.rigidbodyPosition + Vector3.up;
            float viewDistance = _cameraController.orthographicSize * 2 + 2;
            bool isPlayerVisible = _playerController.IsVisibleFromPoint(viewPosition, viewDistance);
            bool isMobVisible = _cameraController.CheckBoundsIntersectsFrustum(mob.view.rigidbodyColliderBounds);

            if (mob.agro01 == 1)
            {
                
            }
            else
            {
                if (isPlayerVisible && isMobVisible)
                {
                    Vector3 toPlayerDirection = _playerController.rigidbodyPosition - mob.view.rigidbodyPosition;
                    float playerDistance = toPlayerDirection.magnitude;

                    mob.view.AlignMeshTowards(toPlayerDirection, _config.mobRotationSpeed, 0.1f);

                    PlayerAimStatus aimStatus = _playerController.GetAimStatus();
                    float aimMobAngle = Vector3.Angle(-toPlayerDirection, aimStatus.aimDirection);
                    if ((aimStatus.isFireAim && aimMobAngle < _config.aimAgroAngle) || aimStatus.isChargingGun)
                    {
                        _audioController.StopEvent(mob.growlAudio);
                        mob.growlAudio = -1;
                        
                        _audioController.StartEvent(AudioEventName.monsterBeginAgro, mob.view);
                        mob.view.SetEmissionColor(Color.red);
                        mob.agro01 = 1;
                    }
                    else if (playerDistance < _config.agroDistance)
                    {
                        if (mob.agro01 == 0 && !_audioController.IsPlaying(mob.growlAudio))
                        {
                            mob.growlAudio = _audioController.StartEvent(AudioEventName.monsterGrowl, mob.view);
                        }

                        mob.agro01 = Mathf.Clamp01(mob.agro01 + Time.deltaTime / _config.agroDelay);

                        if (mob.agro01 == 1)
                        {
                            _audioController.StartEvent(AudioEventName.monsterBeginAgro, mob.view);
                            mob.view.SetEmissionColor(Color.red);
                        }
                    }
                    else
                    {
                        _audioController.StopEvent(mob.growlAudio);
                        mob.growlAudio = -1;
                        mob.agro01 = 0;
                    }
                }
                else
                {
                    _audioController.StopEvent(mob.growlAudio);
                    mob.growlAudio = -1;
                    mob.agro01 = 0;
                }
            }
        }

        private void SpawnMob(Vector3 position)
        {
            CharacterView view = _characterFactory.CreateView(CharacterMeshId.sharpTooth);
            view.RegisterDamageHandler(_damageController);
            view.SetFaction(DamageFactionId.Enemy);
            view.Teleport(position);

            MobData mobData = new()
            {
                view = view,
                agro01 = 0,
                growlAudio = -1,
            };
            _aliveMobs.Add(mobData);
        }
    }
}