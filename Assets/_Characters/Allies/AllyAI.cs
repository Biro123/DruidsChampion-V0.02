using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    public class AllyAI : MonoBehaviour
    {
        [SerializeField] float activateRange = 20f;

        PlayerControl playerControl;
        CombatantAI combatantAI;

        // Use this for initialization
        void Start()
        {
            playerControl = FindObjectOfType<PlayerControl>();
            combatantAI = GetComponent<CombatantAI>();
        }

        // Update is called once per frame
        void Update()
        {
            // if already activated (ie converted to ally) - exit
            if (combatantAI.GetIsEnemy() == false) { return; }

            var distanceToPlayer = (this.transform.position - playerControl.transform.position).magnitude;

            if (distanceToPlayer <= activateRange)
            {
                combatantAI.SetIsEnemy(false);
            }            
        }
    }
}
