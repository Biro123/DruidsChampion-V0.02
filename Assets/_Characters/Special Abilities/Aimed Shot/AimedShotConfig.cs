using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [CreateAssetMenu(menuName = ("RPG/Special Ability/Aimed Shot"))]
    public class AimedShotConfig : AbilityConfig
    {
        [Header("Aimed Shot Specific")]
        [SerializeField] float damageAdj = 50f;
        [SerializeField] float attackAdj = 0f;
        [SerializeField] [Range(0f, 1f)] float armourAvoidAdj = 0.3f;

        public override AbilityBehaviour GetBehaviour(GameObject objectToAttachTo)
        {
            return objectToAttachTo.AddComponent<AimedShotBehaviour>();
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
