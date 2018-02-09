using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.CameraUI
{    
    public class CameraFollow : MonoBehaviour
    {

        private GameObject player;

        // Use this for initialization
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        private void LateUpdate()
        {
            if (player)
            {
                // Lerp smoothes it to deal with bobbing
                transform.position = Vector3.Lerp(transform.position, player.transform.position, 0.05f);
            }
        }
    }

}
