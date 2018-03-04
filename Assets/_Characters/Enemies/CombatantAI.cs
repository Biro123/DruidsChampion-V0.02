using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(OffenceSystem))]
    [RequireComponent(typeof(HealthSystem))]
    public class CombatantAI : MonoBehaviour
    {
        [SerializeField] bool isEnemy = true;
        [Tooltip("Enemies within this range will move to attack range")]
        [SerializeField] float aggroDistance = 10f;

        [Header("Patrolling details")]
        [SerializeField] WaypointContainer patrolPath;
        [SerializeField] float waypointTolerance = 3f;
        [SerializeField] float waypointDwellTime = 0.5f;

        const int COMBATANT_LAYER = 9;

        float currentWeaponRange;

        Character character;
        Transform target = null;
        OffenceSystem weaponSystem;
        Vector3 nextWaypointPos;
        FearDestinations fearDestinations;
        int opponentLayerMask = 0;
        float distanceToTarget = 0f;
        int waypointIndex;
        float currentAggroDistance;

        Transform formationTransform = null;
        Formation formation;

        enum State { attacking, chasing, idle, patrolling, returning, fleeing };
        State state = State.idle;

        public Transform GetTarget()
        {
            return target;
        }

        public bool GetIsEnemy()
        {
            return isEnemy;
        }

        public void SetIsEnemy(bool isEnemyToSet)
        {
            this.isEnemy = isEnemyToSet;
            StopAllCoroutines();
            weaponSystem.StopAttacking();
            target = null;
            weaponSystem.SetTarget(null);
        }

        public void SetFormationPosition(Formation formationToSet, Transform positionToSet)
        {
            formationTransform = positionToSet;
            if (positionToSet == null)   // leader is killed - revert to no-formation 
            {
                formation = null;
                StopAllCoroutines();
            }
            else
            {
                formation = formationToSet;
            }
        }

        public Formation GetFormation()
        {
            return formation;
        }

        public void StartFleeing(float timeToFlee, bool toReturn, GameObject sourceOfFear = null)
        {
            if (!fearDestinations) { return; }
            if (!sourceOfFear)
            {
                sourceOfFear = FindTargetInRange(aggroDistance).gameObject;
                if (!sourceOfFear)
                {
                    sourceOfFear = this.gameObject;
                }
            }

            Vector3 selectedDestination = fearDestinations.GetDestination(this.gameObject, sourceOfFear);
            if (selectedDestination != Vector3.zero )
            {
                StopAllCoroutines();
                weaponSystem.SetTarget(null);
                weaponSystem.StopAttacking();
                state = State.fleeing;
                StartCoroutine(Flee(selectedDestination, transform.position, timeToFlee, toReturn));                
                target = null;
            }
        }

        private IEnumerator Flee(Vector3 destinationToSet, Vector3 returnDestination, float timeToFlee, bool toReturn)
        {
            yield return new WaitForSeconds(0.5f);
            character.SetDestination(destinationToSet);
            yield return new WaitForSeconds(timeToFlee);
            if (toReturn)
            {
                state = State.returning;
                character.SetDestination(returnDestination);
            }
            else
            {
                Destroy(gameObject);
            }            
        }

        private void Start()
        {
            character = GetComponent<Character>();
            weaponSystem = GetComponent<OffenceSystem>();
            fearDestinations = FindObjectOfType<FearDestinations>();
            currentWeaponRange = weaponSystem.GetCurrentWeapon().GetAttackRange();
  
            opponentLayerMask = opponentLayerMask | (1 << COMBATANT_LAYER);
        }

        private void Update()
        {
            if (state == State.fleeing) { return; }

            if (formationTransform == null)
            {
                currentAggroDistance = aggroDistance;
                UpdateWithoutFormation();
            }
            else
            {
                currentAggroDistance = formation.GetFormationAggroDistance();
                UpdateWithoutFormation();
            }
        }

        private void UpdateWithFormation()
        {
            print(gameObject.name + " Updating with formation");
        }

        private void UpdateWithoutFormation()
        {
            target = FindTargetInRange(Mathf.Max(currentAggroDistance, currentWeaponRange));
            distanceToTarget = 0f;
            bool inAttackRange = false;
            bool inAggroRange = false;

            if (target)
            {
                distanceToTarget = (transform.position - target.transform.position).magnitude;
                inAttackRange = (distanceToTarget <= currentWeaponRange);
                inAggroRange = (distanceToTarget <= currentAggroDistance && !inAttackRange);
            }
            else
            {
                inAttackRange = false;
                inAggroRange = false;
            }

            if (!inAttackRange && !inAggroRange)
            {
                NoAggroMovement();
            }

            if (inAggroRange)
            {
                if (state != State.chasing)
                {
                    StopAllCoroutines();
                    weaponSystem.StopAttacking();
                    StartCoroutine(ChaseTarget());
                }
            }

            if (inAttackRange)
            {
                if (state == State.attacking)
                {
                    weaponSystem.ChangeTarget(target.gameObject); // handle target swap from ally to enemy
                }
                else
                {
                    StopAllCoroutines();
                    state = State.attacking;
                    character.SetDestination(transform.position);
                    weaponSystem.StartAttackingTarget(target.gameObject);
                }
            }
        }

        private void NoAggroMovement()
        {
            if (patrolPath && state != State.patrolling)
            {
                StopAllCoroutines();
                weaponSystem.StopAttacking();
                StartCoroutine(Patrol());
            }
            else if (formation)
            {
                ReturnToFormation();
            }
            else if (!patrolPath && !formation)
            {
                state = State.idle;
            }
        }

        private void ReturnToFormation()
        {
            state = State.returning;
            character.SetDestination(formationTransform.position);
        }

        IEnumerator Patrol()
        {
            if (patrolPath == null) { yield return null; }
            state = State.patrolling;

            // Set initial waypoint
            CycleWaypoint();

            while (true)
            {
                var distanceToWaypoint = Vector3.Distance(transform.position, nextWaypointPos);
                if (distanceToWaypoint <= waypointTolerance)
                {
                    // Reached waypoint - so wait then change to next
                    yield return new WaitForSeconds(waypointDwellTime);
                    CycleWaypoint();
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void CycleWaypoint()
        {
            // % = remainder from division.. i don't understand this
            waypointIndex = (waypointIndex + 1) % patrolPath.transform.childCount;
            nextWaypointPos = patrolPath.transform.GetChild(waypointIndex).position;
            character.SetDestination(nextWaypointPos);
        }

        IEnumerator ChaseTarget()
        {
            state = State.chasing;
            while(target && distanceToTarget >= currentWeaponRange )
            {
                character.SetDestination(target.position);
                yield return new WaitForEndOfFrame();
            }
        }

        private Transform FindTargetInRange(float aggroRange)
        {
            // See what are in range
            Collider[] opponentsInRange = Physics.OverlapSphere(this.transform.position, aggroRange, opponentLayerMask);
            if (opponentsInRange.Length == 0) { return null; }

            // Find closest in range
            float closestRange = 0;
            Collider closestTarget = null;
            foreach (var opponentInRange in opponentsInRange)
            {
                if (CheckIfValidTarget(opponentInRange.gameObject) == true)
                {
                    if (target != null && opponentInRange.gameObject == target.gameObject)
                    {  // keep current target if still in range
                        return opponentInRange.transform;
                    }
                    float currentRange = (transform.position - opponentInRange.transform.position).magnitude;
                    if (closestTarget == null || currentRange < closestRange)
                    {
                        closestTarget = opponentInRange;
                        closestRange = currentRange;
                    }
                }
            }
            if (closestTarget)
            {
                return closestTarget.transform;
            }
            else
            {
                return null;
            }
        }

        private bool CheckIfValidTarget(GameObject opponentInRange)
        {
            var currentOpponentCombatantAI = opponentInRange.GetComponent<CombatantAI>();
            
            if (currentOpponentCombatantAI)
            {
                if (currentOpponentCombatantAI.GetIsEnemy() != this.isEnemy)
                { return true; }
                else
                { return false; }
            }
            else  // player targetted
            {
                if (this.isEnemy)   
                { return true; }
                else
                { return false; }
            }
        }

        private void OnDrawGizmos()
        {
            // Draw Attack Sphere
            Gizmos.color = new Color(255f, 0f, 0f);
            Gizmos.DrawWireSphere(transform.position, currentWeaponRange);

            // Draw Move Sphere
            Gizmos.color = new Color(0f, 255f, 0f);
            Gizmos.DrawWireSphere(transform.position, currentAggroDistance);
        }
    }
}
