using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;
using System;

namespace RPG.Characters
{
    public class Formation : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] CombatantAI[] troopers;

        [Tooltip("Casualties Percent taken before fleeing")]
        [SerializeField] [Range(0f, 1f)] float morale = 0.5f;

        [Tooltip("Aggro distance of troopers while in formation")]
        [SerializeField]float formationAggroDistance = 3f;    

        [Header("Initial Move")]
        [SerializeField] Vector3 positionToMoveTo = Vector3.zero;
        [SerializeField] float moveDelay = 3f;
        [SerializeField] float distanceToStartMove = 10f;

        private Transform[] troopPositions;
        private PlayerControl player;
        private bool hasMovementTriggerred = false;
        private bool isEnemy = true;   // Only used for allies


        public float GetFormationAggroDistance()
        {
            return formationAggroDistance;
        }

        public bool GetIsEnemy()
        {
            return isEnemy;
        }
        public void SetIsEnemy(bool isEnemyToSet)
        {
            isEnemy = isEnemyToSet;
            StopFormationAttacking();
        }

        void Start()
        {
            player = FindObjectOfType<PlayerControl>();
            troopPositions = GetComponentsInChildren<Transform>();

            if (troopers.Length > troopPositions.Length)
            {
                Debug.LogError(gameObject.name + " formation has too many troopers assigned");
            }

            // Assign troops to start positions and rotations
            for (int idx = 0; idx <= troopPositions.Length; idx++)
            {
                if (idx < troopers.Length)
                {
                    var currentCombatantAI = troopers[idx];
                    var currentTroopTransform = troopPositions[idx].transform;
                    currentCombatantAI.transform.position = currentTroopTransform.position;
                    currentCombatantAI.transform.rotation = transform.rotation;
                    currentCombatantAI.SetFormationPosition(this, currentTroopTransform);                  
                }
            }
        }


        // Update is called once per frame
        void Update()
        {
            if (troopers[0] == null)  // Leader is destroyed
            {
                foreach (var combatantAI in troopers)
                {
                    if (combatantAI != null)
                    {
                        combatantAI.SetFormationPosition(null, null);
                    }
                }
                Destroy(gameObject);
                return;
            }

            MoraleCheck();

            if(positionToMoveTo != Vector3.zero && !hasMovementTriggerred)
            {
                MoveIfInRange();
            }

            // Set the formation position and rotation of the formnation to that of the leader
            //transform.position = troopers[0].transform.position;
            //transform.rotation = troopers[0].transform.rotation;
        }

        private void MoveIfInRange()
        {
            float distanceToPlayer = (transform.position - player.transform.position).magnitude;
            if(distanceToPlayer <= distanceToStartMove)
            {
                hasMovementTriggerred = true;
                StartCoroutine(MoveAfterDelay());
            }
        }

        private IEnumerator MoveAfterDelay()
        {
            yield return new WaitForSeconds(moveDelay);
            transform.position = positionToMoveTo;
        }

        private void MoraleCheck()
        {
            float remainingTroops = 0f;
            foreach(var trooper in troopers)
            {
                if (trooper)
                {
                    remainingTroops++;
                }
            }

            float casualtyPercent = (1.0f - remainingTroops / troopers.Length);
            float fleeChance = casualtyPercent - morale;
            if (fleeChance > 0f)
            {
                print ("RunAway! " + fleeChance);
                foreach (var survivingTrooper in troopers)
                {
                    if (survivingTrooper)
                    {
                        if (UnityEngine.Random.Range(0f,1f) <= fleeChance*Time.deltaTime)
                        {
                            survivingTrooper.StartFleeing(10f, false);  //TODO Magic Number time to flee
                        }
                    }
                }
            }
        }

        private void StopFormationAttacking()
        {
            foreach (var trooper in troopers)
            {
                if (trooper)
                {
                    trooper.SetIsEnemy(false);
                }
            }
        }

        private void OnDrawGizmos()
        {
            foreach (Transform troopPosition in transform)
            {
                Gizmos.DrawSphere(troopPosition.position, 0.2f);
            }

            Gizmos.color = Color.cyan;
            if (positionToMoveTo != Vector3.zero)
            {
                foreach (Transform troopPosition in transform)
                {
                    Vector3 moveToPosition = positionToMoveTo - transform.position + troopPosition.position;
                    Gizmos.DrawSphere(moveToPosition, 0.2f);
                }
                Gizmos.DrawWireSphere(transform.position, distanceToStartMove);
            }
        }
    }
}
