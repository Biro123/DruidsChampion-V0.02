using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;  

namespace RPG.Characters
{
    public class Formation : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] GameObject row1Trooper;
        [SerializeField] GameObject row2Trooper;
        [SerializeField] GameObject leaderPrefab;

        [Header("Formation Behaviour")]
        [Tooltip("Distance from enemy to reform")]
        [SerializeField] float reformDistance = 15f;
        [SerializeField] int[] layersToTarget = { 10, 11 };

        [Header("Individual Troop settings")]
        [Tooltip("Aggro distance while in formation")]
        [SerializeField]float formationAggroDistance = 3f;

        [Tooltip("Stop Distance while going back to formation")]
        [SerializeField] float formationStopDistance = 0.5f;

        private Transform[] troopPositions;
        private GameObject[] troopers;
        private GameObject leader;
        private int rankSize;

        // Use this for initialization
        void Start()
        {

            troopPositions = GetComponentsInChildren<Transform>();
            troopers = new GameObject[troopPositions.Length];

            int trooperIndex = 0;
            foreach (Transform troopPosition in troopPositions)
            {
                if (troopPosition.position == this.transform.position)
                {
                    AddTrooper(leaderPrefab, trooperIndex, troopPosition);
                }
                else
                {   //TODO - pull in 2nd rank correctly. 
                    AddTrooper(row1Trooper, trooperIndex, troopPosition);
                }
                trooperIndex++;
            }
            rankSize = (troopPositions.Length - 1) / 2;
        }

        private void AddTrooper(GameObject troopPrefab, int trooperIndex, Transform troopPosition)
        {
            GameObject trooper = Instantiate(troopPrefab, troopPosition.position, transform.rotation, transform.parent);
            //trooper.GetComponent<Enemy>().SetOrder(UnitOrder.ShieldWall, troopPosition);
            troopers[trooperIndex] = trooper;
        }

        // Update is called once per frame
        void Update()
        {
            if (troopers[0] == null)  // Leader is destroyed
            {
                foreach (var trooper in troopers)
                {
                    if (trooper != null)
                    {
                       // trooper.GetComponent<Enemy>().SetOrder(UnitOrder.Skirmish, trooper.transform);
                    }

                }
                Destroy(gameObject);
                return;
            }

            // Set the formation position and rotation to that of the leader
            transform.position = troopers[0].transform.position;
            transform.rotation = troopers[0].transform.rotation;
            //transform.localScale = warlord.localScale;

            if (SafeToReform())
            {
                Reform();
            }
            else
            {
                FillGap();
            }
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

            // TODO - face front after reform.. may need to be elsewhere as can only pass transforms
        }

        public void FillGap()
        {   // When opponents near, simply move from rear to front if there is a gap.
            if (row2Trooper == null) { return; }

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
    }
}
