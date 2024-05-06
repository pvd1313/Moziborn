using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moziborn.Layers;
using UnityEngine;

namespace Moziborn.Damage
{
    public class BulletManager
    {
        private struct BulletData
        {
            public BulletPrefab instance;
            public float speed;
            public float damageLeft;
            public float distanceLeft;
        }
        
        private readonly BulletManagerConfig _config;
        
        private Dictionary<string, BulletConfig> _bulletConfigs;
        private List<BulletData> _activeBullets = new List<BulletData>();
        private List<BulletPrefab> _bulletPool;
        private Transform _root;
        
        public BulletManager(BulletManagerConfig config)
        {
            _config = config;
        }
        
        public IEnumerator Initialize()
        {
            _root = new GameObject("BulletManager").transform;
            
            _bulletConfigs = _config.bullets.ToDictionary(config => config.id);
            _bulletPool = new List<BulletPrefab>();
            
            yield break;
        }
        
        public void SpawnBullet(string bulletId, Vector3 position, Vector3 direction)
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
                distanceLeft = bulletConfig.maxDistance
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

                if (Physics.Raycast(ray, travelDistance, 1 << LayerIndex.Default))
                {
                    isHit = true;
                }
                else
                {
                    bullet.instance.transform.position += ray.direction * travelDistance;
                }

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