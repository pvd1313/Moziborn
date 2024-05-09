using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moziborn.Layers;
using UnityEngine;

namespace Moziborn.Damage
{
    public class BulletController
    {
        private struct BulletData
        {
            public BulletPrefab instance;
            public DamageFactionId factionId;
            public float speed;
            public float damageLeft;
            public float distanceLeft;
        }
        
        private readonly BulletControllerConfig _config;
        private readonly DamageController _damageController;

        private Dictionary<string, BulletConfig> _bulletConfigs;
        private List<BulletData> _activeBullets;
        private List<BulletPrefab> _bulletPool;
        private Transform _root;
        
        public BulletController(BulletControllerConfig config,
                                DamageController damageController)
        {
            _config = config;
            _damageController = damageController;
        }
        
        public IEnumerator Initialize()
        {
            _root = new GameObject("BulletManager").transform;
            
            _bulletConfigs = _config.bullets.ToDictionary(config => config.id);
            _bulletPool = new List<BulletPrefab>();
            _activeBullets = new List<BulletData>();
            
            yield break;
        }
        
        public void SpawnBullet(string bulletId, DamageFactionId factionId, Vector3 position, Vector3 direction)
        {
            if (!_bulletConfigs.TryGetValue(bulletId, out BulletConfig bulletConfig))
            {
                throw new InvalidOperationException($"Unknown bullet type: '{bulletId}'");
            }
            
            BulletPrefab bullet = GetOrCreateBullet();
            bullet.transform.position = position;
            bullet.transform.forward = direction;
            bullet.gameObject.SetActive(true);
            
            BulletData bulletData = new ()
            {
                instance = bullet, 
                speed = bulletConfig.speed, 
                damageLeft = bulletConfig.damage,
                distanceLeft = bulletConfig.maxDistance,
                factionId = factionId
            };
            
            _activeBullets.Add(bulletData);
        }

        private BulletPrefab GetOrCreateBullet()
        {
            if (_bulletPool.Count > 0)
            {
                BulletPrefab bullet = _bulletPool[^1];
                _bulletPool.RemoveAt(_bulletPool.Count - 1);
                
                return bullet;
            }
            {
                BulletPrefab bullet = GameObject.Instantiate(_config.bulletPrefab, _root);
                
                return bullet;
            }
        }

        public void Update()
        {
            for (int i = _activeBullets.Count - 1; i >= 0; i--)
            {
                BulletData bullet = _activeBullets[i];

                float travelDistance = bullet.speed * Time.deltaTime;
                
                bullet.distanceLeft -= travelDistance;

                Ray ray = new Ray(
                    bullet.instance.transform.position,
                    bullet.instance.transform.forward);

                bool isHit = false;

                if (Physics.Raycast(ray, out RaycastHit hit, travelDistance, 1 << LayerIndex.Default))
                {
                    if (_damageController.ApplyDamage(hit.collider, bullet.factionId, bullet.damageLeft) > 0)
                    {
                        isHit = true;
                    }
                }
                
                bullet.instance.transform.position += ray.direction * travelDistance;

                if (bullet.distanceLeft <= 0 || isHit)
                {
                    bullet.instance.gameObject.SetActive(false);
                    _activeBullets.RemoveAt(i);
                    _bulletPool.Add(bullet.instance);
                }
                else
                {
                    _activeBullets[i] = bullet;
                }
            }
        }
    }
}