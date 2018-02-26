using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [CreateAssetMenu(menuName = ("RPG/Special Ability/Battle Roar"))]
    public class BattleRoarConfig : AbilityConfig
    {
        [Header("Battle Roar Specific")]
        [SerializeField] float level = 5f;
        [SerializeField] float radius = 8f;
        [SerializeField] float duration = 5f;

        public override AbilityBehaviour GetBehaviour(GameObject objectToAttachTo)
        {
            return objectToAttachTo.AddComponent<BattleRoarBehaviour>();
        }

        public float GetBattleRoarLevel()
        {
            return level;
        }
        public float GetRadius()
        {
            return radius;
        }
        public float GetDuration()
        {
            return duration;
        }
    }
}
