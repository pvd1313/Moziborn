using UnityEngine;

namespace Moziborn.Damage
{
    public class BulletManagerConfig : ScriptableObject
    {
        public BulletPrefab bulletPrefab;
        public BulletConfig[] bullets;
    }
}