using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moziborn.Damage
{
    public class DamageController
    {
        private Dictionary<Collider, IDamageHandler> _damageHandlers;
        
        public IEnumerator Initialize()
        {
            _damageHandlers = new Dictionary<Collider, IDamageHandler>();
            
            yield break;
        }

        public float ApplyDamage(Collider collider, DamageFactionId factionId, float damageAmount)
        {
            if (_damageHandlers.TryGetValue(collider, out IDamageHandler damageHandler))
            {
                if (damageHandler.factionId == factionId)
                {
                    return 0;
                }

                return damageHandler.TakeDamage(damageAmount);
            }
            
            // Static obstacle
            return damageAmount;
        }
        
        public void RegisterCollider(Collider collider, IDamageHandler damageHandler)
        {
            _damageHandlers[collider] = damageHandler;
        }

        public void UnregisterCollider(Collider collider)
        {
            _damageHandlers.Remove(collider);
        }
    }
}