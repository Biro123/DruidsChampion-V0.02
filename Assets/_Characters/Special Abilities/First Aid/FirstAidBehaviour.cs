using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Characters    
{
    public class FirstAidBehaviour : AbilityBehaviour
    {
        PlayerControl player;

        private void Start()
        {
            player = GetComponent<PlayerControl>();
        }

        public override void Use(GameObject target)
        {
            HealPlayer();            
            PlayParticleEffect();
            PlayAbilityAudio();
            PlayAbilityAnimation();
        }

        private void HealPlayer()
        {
            var playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.AdjustHealth( -(config as FirstAidConfig).GetHealAmount() );
            }            
        }
        
    }
}
