using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Characters    
{
    public class AreaEffectBehaviour : AbilityBehaviour
    {
        public override void Use(GameObject target)
        {
            DealRadialDamage();            
            PlayParticleEffect();
            PlayAbilityAudio();
            PlayAbilityAnimation();
        }


        private void DealRadialDamage()
        {
            float damageToDeal = (config as AreaEffectConfig).GetExtraDamage();
            Collider[] collidersInRange = Physics.OverlapSphere(transform.position, (config as AreaEffectConfig).GetRadius());

            foreach (Collider colliderInRange in collidersInRange)
            {
                if (colliderInRange.gameObject.GetInstanceID() != this.gameObject.GetInstanceID())
                {  
                    HealthSystem targetHealthSystem = colliderInRange.GetComponent<HealthSystem>();
                    if (targetHealthSystem != null)
                    {
                        targetHealthSystem.AdjustHealth(damageToDeal);

                    }
                }
            }
        }
    }
}
