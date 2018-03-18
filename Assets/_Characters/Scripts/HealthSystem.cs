using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RPG.Characters
{
    public class HealthSystem : MonoBehaviour
    {

        [SerializeField] float maxHealthPoints = 100f;
        [SerializeField] Image healthBar;
        [SerializeField] float deathVanishSeconds = 2f;
        [SerializeField] AudioClip[] hitSounds;
        [SerializeField] AudioClip[] deathSounds;
        [SerializeField] GameObject bloodSpurtPrefab;

        const string DEATH_TRIGGER = "Death";

        Animator animator;
        AudioSource audioSource;
        Character character;
        float currentHealthPoints = 0;

        public float healthAsPercentage
        {
            get { return currentHealthPoints / maxHealthPoints; }
        }

        public void AdjustHealth(float amount)
        {
            bool isDieingThisHit = (currentHealthPoints > 0); // must ask before reducing health
            ReduceHealth(amount);
            if (currentHealthPoints <= 0f && isDieingThisHit && amount > 0)
            {
                StartCoroutine(KillCharacter());
            }
        }

        public void AdjustHealthPercent(float percentToAdjust, bool useRemainingHealth)
        {
            float healthToReduce = 0;

            if (useRemainingHealth)
            {
                healthToReduce = currentHealthPoints * percentToAdjust;
            }
            else
            {
                healthToReduce = (maxHealthPoints - currentHealthPoints) * percentToAdjust;
            }

            AdjustHealth(healthToReduce);
        }

        void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            character = GetComponent<Character>();
            currentHealthPoints = maxHealthPoints;
        }

        void Update()
        {
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            if(healthBar)
            {
                healthBar.fillAmount = healthAsPercentage;
            }
        }

        private void ReduceHealth(float damage)
        {
            //print(Time.time + gameObject.name + " taking damage now: " + damage);
            currentHealthPoints = Mathf.Clamp(currentHealthPoints - damage, 0f, maxHealthPoints);
            if (damage <= 0) { return; }  // don't play sound if being healed.

            float hitSoundVolume = (damage / maxHealthPoints * 2);   // anything over half health = full volume
            PlayHitSound(Mathf.Clamp(hitSoundVolume, 0, 1));

            SpawnBloodSpurt();
        }

        private void SpawnBloodSpurt()
        {
            Vector3 heightAdjustment = new Vector3(0, 1f, 0);
            Vector3 bloodSpurtPosition = this.transform.position + heightAdjustment;
            float bloodSpurtYRot = UnityEngine.Random.Range(110f, 250f);
            Quaternion bloodSpurtRotation = Quaternion.Euler(0f, bloodSpurtYRot, 0f);
            GameObject bloodSpurt = Instantiate(bloodSpurtPrefab, bloodSpurtPosition, bloodSpurtRotation);
            bloodSpurt.GetComponent<ParticleSystem>().Play();  
            Destroy(bloodSpurt, bloodSpurt.GetComponent<ParticleSystem>().main.duration);
        }

        private void PlayHitSound(float volume)
        {
            if (hitSounds.Length == 0) { return; }

            int audioIndex = UnityEngine.Random.Range(0, hitSounds.Length);
            var clip = hitSounds[audioIndex];
            audioSource.PlayOneShot(clip, volume);
        }

        IEnumerator KillCharacter()
        {
            character.Kill();
            animator.SetTrigger(DEATH_TRIGGER);
            PlayDeathSound();

            if (healthBar) // Hide Canvas
            {
                healthBar.transform.parent.gameObject.SetActive(false);
            }

            var playerComponent = GetComponent<PlayerControl>();
            if (playerComponent && playerComponent.isActiveAndEnabled)
            {                
                yield return new WaitForSeconds(audioSource.clip.length);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                Destroy(gameObject, deathVanishSeconds);
            }
        }
        
        private void PlayDeathSound()
        {
            if (deathSounds.Length == 0) { return; }

            int audioIndex = UnityEngine.Random.Range(0, deathSounds.Length);
            audioSource.clip = deathSounds[audioIndex];
            audioSource.Stop();
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}
