using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters {
    public class HealingHerbPickup : MonoBehaviour
    {
        [SerializeField] float triggerRadius = 1f;
        [SerializeField] int numberOfPacks = 3;
        GameObject player;
        bool isPickedUp = false;

        // Use this for initialization
        void Start()
        {
            player = FindObjectOfType<PlayerControl>().gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            if (isPickedUp) { return; }

            var distanceToPlayer = Vector3.Magnitude(transform.position - player.transform.position);
            if (distanceToPlayer <= triggerRadius)
            {
                player.GetComponent<FirstAidBehaviour>().AddMoreUses(numberOfPacks);
                isPickedUp = true;
            }
        }
    }
}
