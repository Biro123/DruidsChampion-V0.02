using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;  

namespace RPG.Characters
{
    public class Formation : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] GameObject leaderPrefab;
        [SerializeField] CombatantAI[] troopers; 

        [Header("Formation Behaviour")]
        [Tooltip("Distance from enemy to reform")]
        [SerializeField] float reformDistance = 15f;
        [SerializeField] int[] layersToTarget = { 10, 11 };

        [Header("Individual Troop settings")]
        [Tooltip("Aggro distance while in formation")]
        [SerializeField]float formationAggroDistance = 3f;

        private Transform[] troopPositions;
        private GameObject leader;
        private int rankSize;
        

        public float GetFormationAggroDistance()
        {
            return formationAggroDistance;
        }

        void Start()
        {
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
            rankSize = (troopPositions.Length - 1) / 2;
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

            // Set the formation position and rotation of the formnation to that of the leader
            //transform.position = troopers[0].transform.position;
            //transform.rotation = troopers[0].transform.rotation;

            //if (SafeToReform())
            //{
            //    Reform();
            //}
            //else
            //{
            //    FillGap();
            //}
        }

        bool SafeToReform()
        {
            // Set up the layermask to check - ie. look for player or his allies.
            int opponentLayerMask = 0;
            foreach (var layer in layersToTarget)
            {
                opponentLayerMask = opponentLayerMask | (1 << layer);
            }

            // See what are in range
            Collider[] opponentsInRange = Physics.OverlapSphere(this.transform.position, reformDistance, opponentLayerMask);

            if (opponentsInRange.Length == 0)
            {
                return true;
            }
            return false;
        }

        void Reform()
        {   //  When safe - reform to plug gaps and fill from front/centre
            int j = 1;
            for (int i = 1; i <= troopPositions.Length; i++) // Leader is always on index 0
            {
                if (troopers[j] != null)    // Trooper still alive
                {
                    // Move last trooper to first, and set its target to that position
                    troopers[i] = troopers[j];   // ensure index matches with position
                    //troopers[i].GetComponent<Enemy>().SetOrder(UnitOrder.Reform, troopPositions[i]);
                }
                else
                {
                    i--;     // try same position again with next trooper
                }
                j++;
                if (j >= troopers.Length) { break; };   // Exit if all troopers placed
            }

        }

        public void FillGap()
        {   // When opponents near, simply move from rear to front if there is a gap.
            //if (row2Trooper == null) { return; }

            // See if gap in front
            for (int i = 1; i < rankSize; i++)
            {
                if (troopers[i] == null)
                {
                    if (troopers[i + rankSize] != null)
                    { // if so, move from back
                        troopers[i] = troopers[i + rankSize];   // ensure index matches with position
                        //troopers[i].GetComponent<Enemy>().SetFormationPos(troopPositions[i]);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            foreach (Transform troopPosition in transform)
            {
                Gizmos.DrawSphere(troopPosition.position, 0.2f);
            }
        }
    }
}
