using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [CreateAssetMenu(menuName = ("RPG/Special Ability/Feigned Attack"))]
    public class FeignedAttackConfig : AbilityConfig
    {
        [Header("Feigned Attack Specific")]
        [SerializeField] float damageAdj = 50f;
        [SerializeField] float attackAdj = 30f;
        [SerializeField] [Range(-1f, 1f)] float armourAvoidAdj = -0.1f;

        public override AbilityBehaviour GetBehaviour(GameObject objectToAttachTo)
        {
            return objectToAttachTo.AddComponent<FeignedAttackBehaviour>();
        }

        public float GetDamageAdj()
        {
            return damageAdj;
        }

        public float GetAttackAdj()
        {
            return attackAdj;
        }

        public float GetArmourAvoidAdj()
        {
            return armourAvoidAdj;
        }

    }
}
