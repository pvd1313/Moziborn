using UnityEngine;

namespace Moziborn.Damage
{
    public class BulletControllerConfig : ScriptableObject
    {
        public BulletPrefab bulletPrefab;
        public BulletConfig[] bullets;
    }
}