using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RPG.Characters    
{
    public class BattleRoarBehaviour : AbilityBehaviour
    {
        private FearDestinations fearDestinations;
        private float timeToFlee;

        private void Start()
        {
            timeToFlee = (config as BattleRoarConfig).GetDuration();
            fearDestinations = FindObjectOfType<FearDestinations>();
            if (!fearDestinations)
            {
                Debug.LogError("FearDestination missing from scene");
            }
        }

        public override void Use(GameObject target)
        {
            CauseFearInArea();            
            PlayParticleEffect();
            PlayAbilityAudio();
            PlayAbilityAnimation();
        }

        private void CauseFearInArea()
        {
            float fearLevel = (config as BattleRoarConfig).GetBattleRoarLevel();
            Collider[] collidersInRange = Physics.OverlapSphere(transform.position, (config as BattleRoarConfig).GetRadius());

            foreach (Collider colliderInRange in collidersInRange)
            {
                if (IsFearable(colliderInRange.gameObject))
                {
                    int targetLevel = colliderInRange.GetComponent<Character>().GetLevel();
                    float chanceToFear = (fearLevel + 0.5f - targetLevel)*2 / 10;
                    if (UnityEngine.Random.Range(0f, 1f) <= chanceToFear)
                    {
                        colliderInRange.GetComponent<CombatantAI>().StartFleeing(timeToFlee, this.gameObject);                      
                    }
                }
            }
        }

        private bool IsFearable(GameObject target)
        {
            // TODO make formations immune
            var combatant = target.GetComponent<CombatantAI>();
            var healthSystem = target.GetComponent<HealthSystem>();
            if (combatant && healthSystem.isActiveAndEnabled)
            {
                return combatant.GetIsEnemy();
            }
            return false;
        }     
    }
}
