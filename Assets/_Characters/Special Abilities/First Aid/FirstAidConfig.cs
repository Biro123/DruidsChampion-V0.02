using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [CreateAssetMenu(menuName = ("RPG/Special Ability/First Aid"))]
    public class FirstAidConfig : AbilityConfig
    {
        [Header("First Aid Specific")]
        [SerializeField] [Range (0f, 1f)] float healPercent = 0.6f;
        [SerializeField] float safeRadius = 15f;
        [SerializeField] int maxUses = 5;

        //public override void AttachComponentTo(GameObject gameObjectToAttachTo)
        //{
        //    // Adds the ability Behaviour script to the player gameobject
        //    var behaviourComponent = gameObjectToAttachTo.AddComponent<FirstAidBehaviour>();
        //    behaviourComponent.SetConfig(this);
        //    behaviour = behaviourComponent;
        //}

        public override AbilityBehaviour GetBehaviour(GameObject objectToAttachTo)
        {
            return objectToAttachTo.AddComponent<FirstAidBehaviour>();
        }

        public float GetHealPercent()
        {
            return healPercent;
        }
        public float GetSafeRadius()
        {
            return safeRadius;
        }
        public int GetMaxUses()
        {
            return maxUses;
        }
    }
}
