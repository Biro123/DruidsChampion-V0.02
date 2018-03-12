﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Characters
{
    [CreateAssetMenu(menuName = ("RPG/Weapon"))]
    public class WeaponConfig : ScriptableObject
    {        
        [Header("Setup")]
        [SerializeField] GameObject weaponPrefab;
        [SerializeField] AnimatorOverrideController weaponSpecificAnimations;
        [SerializeField] AudioClip[] parrySounds;
        [SerializeField] float timeBetweenAttacks = 1.2f;
        [SerializeField] float damageDelay = 0.5f;
        [SerializeField] float blockDelay = 0.15f;
        [SerializeField] float attackRange = 2f;

        [Header("Stats")]
        [Range(0.1f, 1.2f)] [SerializeField] float quality = 0.8f;
        [Range(0.1f, 1.0f)] [SerializeField] float condition = 0.8f;
        [Range(0f, 2.0f)] [SerializeField] float bladeDamageModifier = 0.5f;
        [Range(0f, 2.0f)] [SerializeField] float bluntDamageModifier = 0.5f;
        [Range(0f, 2.0f)] [SerializeField] float pierceDamageModifier = 0.5f;

        public Transform gripTransform;

        public AnimatorOverrideController GetWeaponSpecificAnimations()
        {
            return weaponSpecificAnimations;
        }

        public GameObject GetWeaponPrefab()
        {
            return weaponPrefab;
        }
               
        public AudioClip GetParrySound()
        {
            if (parrySounds.Length > 0)
            {
                return parrySounds[Random.Range(0, parrySounds.Length)];
            }
            else
            {
                return null;
            }
        }

        public float GetBlockDelay()
        {
            return blockDelay;
        }

        public float GetTimeBetweenAttacks()
        {
            return timeBetweenAttacks;
        }

        public float GetDamageDelay()
        {
            return damageDelay;
        }

        public float GetAttackRange()
        {
            return attackRange;
        }

        public float GetBladeDamageModification()
        {
            return bladeDamageModifier * quality * condition;
        }

        public float GetBluntDamageModification()
        {
            return bluntDamageModifier * quality * condition;
        }

        public float GetPierceDamageModification()
        {
            return pierceDamageModifier * quality * condition;
        }

        public float GetChanceForSwing()
        {
            float mainSwingDamageMod = Mathf.Max(bladeDamageModifier, bluntDamageModifier);
            float chanceForSwing = mainSwingDamageMod / (mainSwingDamageMod + pierceDamageModifier);
            return chanceForSwing;
        }
    }
}
