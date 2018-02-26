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
        private float minDestinationDistance;
        private float timeToFlee;

        private void Start()
        {
            fearDestinations = Instantiate( (config as BattleRoarConfig).GetFearDestinations());
            minDestinationDistance = (config as BattleRoarConfig).GetMinDistance();
            timeToFlee = (config as BattleRoarConfig).GetDuration();
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
                    //float chanceToFear = Mathf.Pow((fearLevel + 1 - targetLevel), 2) / 10;
                    float chanceToFear = (fearLevel + 0.5f - targetLevel)*2 / 10;
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
            Vector3 selectedFearLocation = Vector3.zero;
            
            foreach (Transform fearLocation in fearDestinations.transform)
            {
                if ((transform.position - fearLocation.position).magnitude >= minDestinationDistance)
                {
                    if (fearedTarget.GetComponent<Character>().IsDestinationReachable(fearLocation.position))
                    {
                        selectedFearLocation = fearLocation.position;  // Possible destination                        
                        var distToFearDest = (fearLocation.position - transform.position).magnitude;
                        var distTargetToFearDest = (fearLocation.position - fearedTarget.transform.position).magnitude;
                        if (distToFearDest > distTargetToFearDest)
                        {
                            selectedFearLocation = fearLocation.position; // Definite destination
                            break;
                        }
                    }
                }
            }
            if (selectedFearLocation != Vector3.zero)
            {
                fearedTarget.GetComponent<CombatantAI>().StartFleeing(selectedFearLocation, timeToFlee);
            }
            else
            {
                Debug.LogWarning("Fear Destinatoin not found");
            }
        }
    }
}
