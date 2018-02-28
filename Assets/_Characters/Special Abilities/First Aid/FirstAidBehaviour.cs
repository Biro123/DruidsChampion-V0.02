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
        CombatantAI combatantAI;
        int remainingUses;
        float safeRadius;
        float healPercent;

        public int GetRemainingUses ()
        {
            return remainingUses;
        }

        public void AddMoreUses(int usesToAdd)
        {
            if (remainingUses + usesToAdd <= (config as FirstAidConfig).GetMaxUses())
            {
                remainingUses = remainingUses + usesToAdd;
            }
            else
            {
                remainingUses = (config as FirstAidConfig).GetMaxUses();
            }
        }

        private void Start()
        {
            player = GetComponent<PlayerControl>();
            remainingUses = (config as FirstAidConfig).GetMaxUses();
            safeRadius = (config as FirstAidConfig).GetSafeRadius();
            healPercent = (config as FirstAidConfig).GetHealPercent();
        }

        public override void Use(GameObject target)
        {
            if ( player.IsSafeDistance(safeRadius) && remainingUses > 0)
            {
                HealPlayer();
                PlayParticleEffect();
                PlayAbilityAudio();
                PlayAbilityAnimation();
            }
        }

        private void HealPlayer()
        {
            var playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                // Heals a percentage of the health that has been lost.
                playerHealth.AdjustHealthPercent( -healPercent, false);
                remainingUses--;
            }            
        }
    }
}
