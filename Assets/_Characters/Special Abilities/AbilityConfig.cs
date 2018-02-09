using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Core;

namespace RPG.Characters
{
    public abstract class AbilityConfig : ScriptableObject
    {
        [Header("Special Ability General")]
        [SerializeField] float staminaCost = 20f;
        [SerializeField] GameObject particlePrefab = null;
        [SerializeField] AudioClip[] audioClips = null;
        [SerializeField] AnimationClip abilityAnimation;

        protected AbilityBehaviour behaviour;  // protected allows only children to access it

        public abstract AbilityBehaviour GetBehaviour(GameObject objectToAttachTo);

        public void AttachAbilityTo(GameObject objectToAttachTo)
        {
            var behaviourComponent = GetBehaviour(objectToAttachTo);
            behaviourComponent.SetConfig(this);
            behaviour = behaviourComponent;
        }

        public void Use(GameObject target)
        {
            behaviour.Use(target);
        }

        public float GetStaminaCost()
        {
            return staminaCost;
        }

        public GameObject GetParticlePrefab()
        {
            return particlePrefab;
        }

        public AudioClip GetRandomAbilitySound()
        {
            return audioClips[Random.Range(0, audioClips.Length) ];
        }

        public AnimationClip GetAbilityAnimation()
        {
            abilityAnimation.events = new AnimationEvent[0];
            return abilityAnimation;
        }

    }
}
