using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    public class FearDestinations : MonoBehaviour
    {
        const float MIN_DISTANCE_OF_DESTINATION = 25f;

        public Vector3 GetDestination(GameObject fearedTarget, GameObject sourceOfFear)
        {
            Vector3 selectedFearLocation = Vector3.zero;

            foreach (Transform fearLocation in transform)
            {
                float distanceFromTarget = (fearedTarget.transform.position - fearLocation.position).magnitude;
                if ( distanceFromTarget >= MIN_DISTANCE_OF_DESTINATION)
                {
                    if (fearedTarget.GetComponent<Character>().IsDestinationReachable(fearLocation.position))
                    {
                        selectedFearLocation = fearLocation.position;  // Possible destination                        
                        var sourceToDest = (fearLocation.position - sourceOfFear.transform.position).magnitude;
                        var targetToDest = (fearLocation.position - fearedTarget.transform.position).magnitude;
                        if (sourceToDest > targetToDest)
                        {
                            selectedFearLocation = fearLocation.position; // Definite destination
                            break;
                        }
                    }
                }
            }
            return selectedFearLocation;
        }
    }
}
