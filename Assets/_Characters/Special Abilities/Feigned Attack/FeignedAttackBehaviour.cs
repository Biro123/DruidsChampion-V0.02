using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Characters
{
    public class FeignedAttackBehaviour : AbilityBehaviour 
    {
        ParticleSystem myParticleSystem;

        public override void Use(GameObject target)
        {
            DealDamage(target);
            PlayWeaponTrail();
            PlayParticleEffect();
            PlayAbilityAudio();
            PlayAbilityAnimation();
        }

        private void DealDamage(GameObject target)
        {
            if (!target) { return; };

            GetComponent<WeaponSystem>().SpecialAttack(
                target, 
                (config as FeignedAttackConfig).GetAttackAdj(),
                (config as FeignedAttackConfig).GetDamageAdj(),
                (config as FeignedAttackConfig).GetArmourAvoidAdj() 
                );
        }        
    }
}
