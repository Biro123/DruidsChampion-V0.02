using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [RequireComponent(typeof(Character))]

    public class CritterAI : MonoBehaviour
    {
        [SerializeField] float distanceBeforeRun = 10f;
        [SerializeField] WaypointContainer patrolPath;
        [SerializeField] float waypointTolerance = 3f;
        [SerializeField] float waypointDwellTime = 0.5f;
        [Tooltip ("Domestic will follow patrol, Wild will use it to flee")]
        [SerializeField] bool isDomestic = false;

        const int COMBATANT_LAYER = 9;

        Character character;
        Transform target = null;
        int opponentLayerMask = 0;
        int waypointIndex;
        Vector3 nextWaypointPos;
        bool isFleeing;
        PlayerControl player;

        private void Start()
        {
            player = FindObjectOfType<PlayerControl>();
            isFleeing = false;
            character = GetComponent<Character>(); 
            opponentLayerMask = opponentLayerMask | (1 << COMBATANT_LAYER);

            if (isDomestic && patrolPath)
            {
                StartCoroutine(Patrol());
            }       
        }

        private void Update()
        {
            if (isDomestic) { return; }

            var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= distanceBeforeRun)
            {
                if (!isFleeing)
                {
                    isFleeing = true;
                    StartCoroutine(Patrol());
                }
            }
            else
            {
                isFleeing = false;
                StopAllCoroutines();
            }
        }

        IEnumerator Patrol()
        {
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
        
        private void OnDrawGizmos()
        {
            // Draw Move Sphere
            Gizmos.color = new Color(0f, 255f, 0f);
            Gizmos.DrawWireSphere(transform.position, distanceBeforeRun);
        }
    }
}
