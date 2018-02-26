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
        Formation formation;

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
            formation = combatantAI.GetFormation();

            if (distanceToPlayer <= activateRange || IsInAlliedFormation())
            {
                combatantAI.SetIsEnemy(false);                
            }
            
            if(distanceToPlayer <= activateRange && !IsInAlliedFormation())
            {
                SetFormationToAllied();
            }
        }

        bool IsInAlliedFormation()
        {
            if (formation && !formation.GetIsEnemy() )
            {
                return true;
            }
            return false;
        }

        void SetFormationToAllied()
        {
            if (formation)
            {
                formation.SetIsEnemy(false);
            }
        }
    }
}
