using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    public class DefenceSystem : MonoBehaviour
    {
        [SerializeField] float blockBonus = 110f;

        [SerializeField] ArmourConfig headArmourConfig;
        [SerializeField] ArmourConfig bodyArmourConfig;
        [SerializeField] ArmourConfig armArmourConfig;
        [SerializeField] ArmourConfig legArmourConfig;

        ArmourConfig armourConfigHit;
        HealthSystem healthSystem;
        OffenceSystem offenceSystem;
        Animator animator;
        AudioSource audioSource;

        bool isBlocking;
        string displayHitLocation;

        const string BLOCK_TRIGGER = "Block";

        public struct ArmourProtection
        {
            public float blade;
            public float blunt;
            public float pierce;
        }

        private void Start()
        {
            healthSystem = GetComponent<HealthSystem>();
            offenceSystem = GetComponent<OffenceSystem>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }

        public float GetBlockBonus() { return blockBonus; }


        public void DefendAgainstAttack(
            float attackScore,
            float defencePenaly,
            float bluntDamageDone, 
            float bladeDamageDone, 
            float pierceDamageDone,
            float armourAvoidAdj,
            float damageDelay)
        {
            if (SeeIfHit(attackScore, defencePenaly) == true)
            {
                float damageTaken = AdjustDamageForArmour(bluntDamageDone, bladeDamageDone, pierceDamageDone, armourAvoidAdj);
                StartCoroutine(TakeDamageAfterDelay(damageTaken, damageDelay));
                //print(Time.time + gameObject.name + " Takes Hit for " + damageTaken + " dmg delay " + damageDelay);
            }
            else
            {
                if (!isBlocking)
                {
                    //blockDelay should relate to the animation time of the weapon used to block 
                    float BlockDelay = offenceSystem.GetCurrentWeapon().GetBlockDelay();
                    float blockDelay = Mathf.Clamp(damageDelay - BlockDelay, 0f, 1f);
                    StartCoroutine(HandleBlockAfterDelay(blockDelay));
                    //print(Time.time + gameObject.name + " Blocks - blockdelay =  " + blockDelay);
                }
                else
                {
                    //print(Time.time + gameObject.name + " already blocking"); 
                }
            }
        }

        private float AdjustDamageForArmour(float bluntDamageDone, float bladeDamageDone, float pierceDamageDone, float armourAvoidAdj)
        {
            ArmourProtection armour = new ArmourProtection();
            armour = CalculateArmour(armourAvoidAdj);

            float bluntDamageTaken = Mathf.Clamp(bluntDamageDone - armour.blunt, 0f, bluntDamageDone);
            float bladeDamageTaken = Mathf.Clamp(bladeDamageDone - armour.blade, 0f, bladeDamageDone);
            float pierceDamageTaken = Mathf.Clamp(pierceDamageDone - armour.pierce, 0f, pierceDamageDone);

            Debug.Log(name + "'s" + displayHitLocation + "is hit for: "
                + " Blunt " + bluntDamageTaken + "(-" + armour.blunt
                + "), Blade " + bladeDamageTaken + "(-" + armour.blade
                + "), Pierce " + pierceDamageTaken + "(-" + armour.pierce
                + ")");
            return bluntDamageTaken + bladeDamageTaken + pierceDamageTaken;
        }

        IEnumerator TakeDamageAfterDelay(float damage, float delay)
        {
            yield return new WaitForSeconds(delay);
            healthSystem.AdjustHealth(damage);
        }

        private bool SeeIfHit(float attackScore, float defencePenalty)
        {
            //defencePenalty is a percentage directional penalty.
            float adjustedDefence = blockBonus * (1 - defencePenalty);
            float defenceScore = UnityEngine.Random.Range(1, 100) + adjustedDefence;
            //print(Time.time + gameObject.name + " attack: " + attackScore + "  defence: " + defenceScore);
            return (attackScore > defenceScore);
        }
        
        IEnumerator HandleBlockAfterDelay(float delay)
        {
            isBlocking = true;
            yield return new WaitForSeconds(delay);
            Block();
            yield return new WaitForSeconds(0.5f);  //TODO another magic number.. time to finish block anim.. 
            isBlocking = false;
        }

        void Block()
        {
            animator.SetTrigger(BLOCK_TRIGGER);            
            audioSource.volume = UnityEngine.Random.Range(0.5f, 1f);
            audioSource.PlayOneShot(offenceSystem.GetCurrentWeapon().GetParrySound());
        }

        private ArmourProtection CalculateArmour(float armourAvoidAdj)
        {
            ArmourProtection armourProtection = new ArmourProtection();

            armourConfigHit = DetermineLocationHit();

            if (armourConfigHit && IsArmourHit(armourAvoidAdj)) // Armour Bypassed (critical)
            {
                armourProtection.blade = armourConfigHit.GetBladeArmourAmount();
                armourProtection.blunt = armourConfigHit.GetBluntArmourAmount();
                armourProtection.pierce = armourConfigHit.GetPierceArmourAmount();
            }

            return armourProtection;
        }

        private ArmourConfig DetermineLocationHit()
        {
            var randomLocation = UnityEngine.Random.Range(0, 100);

            if (randomLocation <= 20) {
                displayHitLocation = " head ";
                return headArmourConfig;
            }
            else if (randomLocation <= 70) {
                displayHitLocation = " body ";
                return bodyArmourConfig;
            }
            else if (randomLocation <= 90) {
                displayHitLocation = " arm ";
                return armArmourConfig;
            }
            else {
                displayHitLocation = " leg ";
                return legArmourConfig;
            }
        }

        private bool IsArmourHit(float armourAvoidAdj)
        {
            float chanceToHitArmour = Mathf.Clamp(armourConfigHit.GetArmourCoverage() - armourAvoidAdj, 0f, 1f);
            return UnityEngine.Random.Range(0f, 1f) <= chanceToHitArmour;
        }
    }
}
