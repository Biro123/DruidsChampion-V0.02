using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [CreateAssetMenu(menuName = ("RPG/Special Ability/First Aid"))]
    public class FirstAidConfig : AbilityConfig
    {
        [Header("First Aid Specific")]
        [SerializeField] float healAmount = 100f;
        [SerializeField] float safeRadius = 15f;

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

        public float GetHealAmount()
        {
            return healAmount;
        }
        public float GetSafeRadius()
        {
            return safeRadius;
        }

    }
}
