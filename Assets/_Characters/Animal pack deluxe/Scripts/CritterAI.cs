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

        //TODO try to do this without layers
        [SerializeField] int[] layersToRunFrom = { 10 };

        Character character;
        Transform target = null;
        int opponentLayerMask = 0;
        int waypointIndex;
        Vector3 nextWaypointPos;

        private void Start()
        {
            character = GetComponent<Character>(); 

            // Set up the layermask of opponents to look for.
            foreach (var layer in layersToRunFrom)
            {
                opponentLayerMask = opponentLayerMask | (1 << layer);
            }

            if (isDomestic && patrolPath)
            {
                Debug.Log(Time.time + " " + gameObject.name + " Starting Coroutine");
                StartCoroutine(Patrol());
            }       
        }

        private void Update()
        {
            if (isDomestic) { return; }

            target = FindTargetInRange(Mathf.Max(distanceBeforeRun ));

            if (target)
            {
                var distanceToTarget = (transform.position - target.transform.position).magnitude;
                if (distanceToTarget <= distanceBeforeRun)
                {
                    CycleWaypoint();
                }
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
            return closestTarget.transform;
        }

        private void OnDrawGizmos()
        {
            // Draw Move Sphere
            Gizmos.color = new Color(0f, 255f, 0f);
            Gizmos.DrawWireSphere(transform.position, distanceBeforeRun);
        }
    }
}
