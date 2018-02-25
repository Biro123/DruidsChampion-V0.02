using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Characters    
{
    public class BattleRoarBehaviour : AbilityBehaviour
    {
        private GameObject fearDestinations;

        private void Start()
        {
            fearDestinations = Instantiate( (config as BattleRoarConfig).GetFearDestinations());
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
                    float chanceToFear = Mathf.Pow((fearLevel + 1 - targetLevel), 2) / 10;
                    Debug.Log(colliderInRange.gameObject.name + " chanceToFear = " + chanceToFear);
                    if (UnityEngine.Random.Range(0f, 1f) <= chanceToFear)
                    {
                        RunAway(colliderInRange.gameObject);                        
                    }
                }
            }
        }

        private bool IsFearable(GameObject target)
        {
            // TODO make formations non-fearable
            var combatant = target.GetComponent<CombatantAI>();
            var healthSystem = target.GetComponent<HealthSystem>();
            if (combatant && healthSystem.isActiveAndEnabled)
            {
                return combatant.GetIsEnemy();
            }
            return false;
        }

        private void RunAway(GameObject fearedTarget)
        {        
            GameObject selectedFearLocation;
            foreach(Transform fearLocation in fearDestinations.transform)
            {
                // TODO check if far enough away
                // TODO check if reachable (possible hit)
                // TODO check if in good direction (definite hit)
                selectedFearLocation = fearLocation.gameObject;
                Debug.Log(fearedTarget.name + " is running to " + selectedFearLocation);
                // call combatantAI to flee (needs new methods.logic)
                break;
            }
        }
    }
}
