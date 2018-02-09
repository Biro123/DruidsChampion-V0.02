using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    public class WaypointContainer : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDrawGizmos()
        {
            Vector3 firstPosition = Vector3.zero;
            Vector3 previousPosition = Vector3.zero;
            foreach (Transform waypoint in transform)
            {
                Gizmos.DrawSphere(waypoint.position, 0.2f);
                if (firstPosition == Vector3.zero)  // First time in
                {
                    firstPosition = waypoint.position;
                    previousPosition = waypoint.position;
                }
                else
                {
                    Gizmos.DrawLine(waypoint.position, previousPosition);
                    previousPosition = waypoint.position;
                }
            }

            if (previousPosition != Vector3.zero && firstPosition != Vector3.zero)
            {
                Gizmos.DrawLine(previousPosition, firstPosition);
            }
        }
    }
}
