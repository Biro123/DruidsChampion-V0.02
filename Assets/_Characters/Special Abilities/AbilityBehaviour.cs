using System;
using System.Collections;
using UnityEngine;

namespace RPG.Characters
{
    public abstract class AbilityBehaviour : MonoBehaviour
    {
        protected AbilityConfig config = null;   // Protected can be accessed by children

        AudioSource audioSource = null;
        AudioClip audioclip = null;

        const float PARTICLE_CLEAN_UP_DELAY = 2f;
        const string SPECIAL_ABILITY_TRIGGER = "SpecialAbility";
        const string SPECIAL_ABILITY = "DEFAULT SPECIAL ABILITY";

        // Abstract class is overridden in the child classes.
        public abstract void Use(GameObject target = null);

        public void SetConfig(AbilityConfig configToSet)
        {
            config = configToSet;
        }

        protected void PlayAbilityAnimation()
        {
            var animatorOverrideController = GetComponent<Character>().GetAnimatorOverrideController();
            var animationToPlay = config.GetAbilityAnimation();

            if (!animationToPlay) { return; }

            if (!animatorOverrideController)
            {
                Debug.Break();
                Debug.LogAssertion("Please provide " + gameObject + " with an animator Override Controller");
            }
            else
            {
                Animator animator = GetComponent<Animator>();                
                animator.runtimeAnimatorController = animatorOverrideController;
                animatorOverrideController[SPECIAL_ABILITY] = animationToPlay;                
                animator.SetTrigger(SPECIAL_ABILITY_TRIGGER);
            }
        }


        protected void PlayAbilityAudio()
        {
            audioclip = config.GetRandomAbilitySound();
            if (audioclip != null)
            {
                audioSource = GetComponent<AudioSource>();
                audioSource.PlayOneShot(audioclip);
            }
        }

        protected void PlayParticleEffect()
        {
            var particlePrefab = Instantiate(config.GetParticlePrefab(), this.gameObject.transform);
            particlePrefab.GetComponent<ParticleSystem>().Play();
            StartCoroutine(DestroyParticleWhenFinished(particlePrefab));
        }

        protected void PlayWeaponTrail()
        {
            var trailAttachpoint = GetComponentInChildren<WeaponTrailAttach>();
            if (!trailAttachpoint) { return; }

            var abilityTrail = config.GetWeaponTrail();
            if (!abilityTrail) { return; }

            var trailPrefab = Instantiate(abilityTrail, trailAttachpoint.gameObject.transform);
            abilityTrail.GetComponent<ParticleSystem>().Play();
            
            StartCoroutine(DestroyParticleWhenFinished(trailPrefab));
        }


        IEnumerator DestroyParticleWhenFinished(GameObject particlePrefab)
        {
            while (particlePrefab.GetComponent<ParticleSystem>().isPlaying)
            {
                yield return new WaitForSeconds(PARTICLE_CLEAN_UP_DELAY);
            }
            Destroy(particlePrefab);
            yield return new WaitForEndOfFrame();
        }
    }
}
