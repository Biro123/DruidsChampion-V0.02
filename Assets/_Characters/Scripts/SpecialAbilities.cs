using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Characters
{
    public class SpecialAbilities : MonoBehaviour
    {
        [SerializeField] Image staminaBarImage;
        [SerializeField] float maxStamina = 100f;
        [SerializeField] float recovPerSecond = 5f;
        [SerializeField] AudioClip outOfStamClip;

        [SerializeField] AbilityConfig[] abilities;

        AudioSource audioSource;

        float currentStamina = 0f;
        
        public int GetNumberOfAbilities ()
        {
            return abilities.Length;
        }

        public void UseStamina(float amount)
        {
            float newStamina = currentStamina - amount;
            currentStamina = Mathf.Clamp(newStamina, 0, maxStamina);
            SetStaminaBar();
        }

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            AttachInitialAbilities();
            currentStamina = maxStamina;
            SetStaminaBar();
        }

        private void Update()
        {
            if (currentStamina < maxStamina)
            {
                RecoverStamina();
                SetStaminaBar();
            }
        }

        void AttachInitialAbilities()
        {
            for (int abilityIndex = 0; abilityIndex < abilities.Length; abilityIndex++)
            {
                // Add the behaviour script to the player.
                abilities[abilityIndex].AttachAbilityTo(this.gameObject);
            }
        }

        public void AttemptSpecialAbility(int abilityIndex, GameObject target = null)
        {
            float staminaCost = abilities[abilityIndex].GetStaminaCost();
            if (staminaCost <= currentStamina)
            {
                UseStamina(staminaCost);
                print("using special ability " + abilityIndex);
                abilities[abilityIndex].Use(target);
            }
            else
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(outOfStamClip);
                }
            }
        }

        private void RecoverStamina()
        {
            float staminaToAdd = recovPerSecond * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina + staminaToAdd, 0, maxStamina);
        }

        private void SetStaminaBar()
        {
            var staminaAsPercentage = currentStamina / maxStamina;
            if (staminaBarImage)
            {
                staminaBarImage.fillAmount = staminaAsPercentage;
            }
        }
    }
}

