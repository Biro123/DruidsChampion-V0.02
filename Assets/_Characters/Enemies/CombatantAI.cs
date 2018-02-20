using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(WeaponSystem))]
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
        WeaponSystem weaponSystem;
        Vector3 nextWaypointPos;
        int opponentLayerMask = 0;
        float distanceToTarget = 0f;
        int waypointIndex;
        float currentAggroDistance;

        Vector3 formationPosition = Vector3.zero;
        Formation formation;

        enum State { attacking, chasing, idle, patrolling, returning };
        State state = State.idle;

        public bool GetIsEnemy()
        {
            return isEnemy;
        }

        public void SetIsEnemy(bool isEnemyToSet)
        {
            this.isEnemy = isEnemyToSet;
        }

        public void SetFormationPosition(Formation formationToSet, Vector3 position)
        {
            formationPosition = position;
            if (position == Vector3.zero)   // leader is killed - rever to no-formation 
            {
                formation = null;
                StopAllCoroutines();
            }
            else
            {
                formation = formationToSet;
            }
        }

        private void Start()
        {
            character = GetComponent<Character>();
            weaponSystem = GetComponent<WeaponSystem>();
            currentWeaponRange = weaponSystem.GetCurrentWeapon().GetAttackRange();
  
            opponentLayerMask = opponentLayerMask | (1 << COMBATANT_LAYER);
        }

        private void Update()
        {
            if (formationPosition == Vector3.zero)
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
            bool inMaxAttackRange = false;
            bool inAggroRange = false;

            if (target)
            {
                distanceToTarget = (transform.position - target.transform.position).magnitude;
                inAttackRange = (distanceToTarget <= currentWeaponRange);
                inAggroRange = (distanceToTarget <= currentAggroDistance && !inAttackRange);
                inMaxAttackRange = (distanceToTarget <= currentWeaponRange + 0.5f);  // TODO magic number - not needed?
            }
            else
            {
                inAttackRange = false;
                inAggroRange = false;
                inMaxAttackRange = false;
            }

            if (!inAttackRange && !inAggroRange)
            {
                if (state != State.patrolling)
                {
                    StopAllCoroutines();
                    weaponSystem.StopAttacking();
                    if (patrolPath)
                    {
                        StartCoroutine(Patrol());
                    }
                    else
                    {
                        state = State.idle;
                    }
                }
            }

            if (inAggroRange)
            {
                // avoid dipping in and out of combat with slight movements //TODO IS NEEDED?
                //if (inMaxAttackRange && state == State.attacking) { return; }

                if (state != State.chasing)
                {
                    StopAllCoroutines();
                    Debug.Log(gameObject.name + " starting attack");
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
                    weaponSystem.AttackTarget(target.gameObject);
                }
            }
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
            while(distanceToTarget >= currentWeaponRange )
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
                if (currentOpponentCombatantAI.GetIsEnemy() != isEnemy)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else  // player targetted
            {
                if (isEnemy)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
