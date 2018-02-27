using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace RPG.Characters
{
    public class WeaponSystem : MonoBehaviour
    {
        [SerializeField] float attackBonus = 100f;
        [SerializeField] float parryBonus = 110f;
        [SerializeField] float baseDamage = 220f;
        [SerializeField] WeaponConfig currentWeaponConfig;

        GameObject target;
        HealthSystem targetHealthSystem;
        GameObject weaponObject;
        Animator animator;
        Character character;
        GlobalCombatConfig globalCombatConfig;

        float lastHitTime;
        bool attackerIsAlive;
        bool targetIsAlive;
        bool isAttacking;

        const string ATTACK_TRIGGER = "Attack";
        const string DEFAULT_ATTACK = "DEFAULT ATTACK";
        const float ATTACK_RANGE_TOLERANCE = 0.5f;

        public float GetParryBonus() {  return parryBonus; }

        void Start()
        {
            animator = GetComponent<Animator>();
            character = GetComponent<Character>();
            globalCombatConfig = FindObjectOfType<GlobalCombatConfig>();

            if(!globalCombatConfig)
            {
                Debug.LogError("Global Combat Config missing from scene");
            }

            PutWeaponInHand(currentWeaponConfig);
            SetAttackAnimation(currentWeaponConfig.GetSwingAnimClip());   // TODO sets starting animation override - including movement
        }

        private void Update()
        {
            bool targetInRange;

            attackerIsAlive = GetComponent<HealthSystem>().healthAsPercentage >= Mathf.Epsilon;

            if (target == null)
            {
                targetIsAlive = false;
                targetInRange = false;
            }
            else
            {
                targetIsAlive = targetHealthSystem.healthAsPercentage >= Mathf.Epsilon;

                float distanceToTarget = Vector3.Distance(this.transform.position, target.transform.position);
                targetInRange = (distanceToTarget <= currentWeaponConfig.GetAttackRange() + ATTACK_RANGE_TOLERANCE); 
            }

            if (target && attackerIsAlive && targetInRange)
            {
                transform.LookAt(target.transform);
            }
            else
            {
                if (isAttacking)
                {
                    StopAttacking();
                }
            }
        }

        public WeaponConfig GetCurrentWeapon()
        {
            return currentWeaponConfig;
        }

        public void PutWeaponInHand(WeaponConfig weaponToUse)
        {
            currentWeaponConfig = weaponToUse;
            var weaponPrefab = weaponToUse.GetWeaponPrefab();
            GameObject dominantHand = RequestDominantHand();
            Destroy(weaponObject);
            weaponObject = Instantiate(weaponPrefab, dominantHand.transform);
            weaponObject.transform.localPosition = currentWeaponConfig.gripTransform.localPosition;
            weaponObject.transform.localRotation = currentWeaponConfig.gripTransform.localRotation;
        }

        private GameObject RequestDominantHand()
        {
            var dominantHands = GetComponentsInChildren<DominantHand>();
            int numberOfDominantHands = dominantHands.Length;

            // Ensure either 1 dominant hand - or an error is returned. 
            Assert.AreNotEqual(numberOfDominantHands, 0, "No Dominant Hand on " + gameObject.name);
            Assert.IsFalse(numberOfDominantHands > 1, "Multiple Dominant Hands on " + gameObject.name);
            return dominantHands[0].gameObject;
        }

        public void StopAttacking()
        {
            StopAllCoroutines();
            animator.StopPlayback();
            target = null;
            targetHealthSystem = null;
            isAttacking = false;
        }

        public void SetTarget(GameObject targetToSet) 
        {
            // Required to ensure that co-routines are not immediately stopped when AttackTarget is called.
            target = targetToSet;
            if (target)
            {
                targetHealthSystem = target.GetComponent<HealthSystem>();
            }
            else
            {
                targetHealthSystem = null;
                StopAttacking();
            }
        }

        public void ChangeTarget(GameObject targetToSet)
        {
            if (target && target != targetToSet)
            {
                StopAllCoroutines();
                AttackTarget(targetToSet);
            }
        }

        public void AttackTarget(GameObject targetToAttack)
        {
            SetTarget(targetToAttack);
            if (!isAttacking)
            {
                float startDelay = 0f;
                if (GetComponent<CombatantAI>()) // slightly delay start of enemy attacking
                {
                    startDelay = currentWeaponConfig.GetDamageDelay();
                }
                StartCoroutine(AttackTargetRepeatedly(startDelay));
            }
        }

        public void SpecialAttack(GameObject targetToAttack, float attackAdj, float damageAdj, float armourAvoidAdj, AnimationClip specialAttackAnimation = null)
        {
            SetTarget(targetToAttack);
            AttackTargetOnce(attackAdj, damageAdj, armourAvoidAdj, specialAttackAnimation);

            if (!isAttacking)
            {
                StartCoroutine(AttackTargetRepeatedly(specialAttackAnimation.length));
            }
        }


        IEnumerator AttackTargetRepeatedly(float startDelay)
        {
            isAttacking = true;
            if(startDelay != 0f)   
            {
                yield return new WaitForSeconds(startDelay);
            }

            while (attackerIsAlive && targetIsAlive)
            {
                var animationClip = currentWeaponConfig.GetSwingAnimClip();
                float animationClipTime = animationClip.length / character.GetAnimSpeedMultiplier();
                float randomDelay = currentWeaponConfig.GetTimeBetweenAnimationCycles() 
                    * (1f + UnityEngine.Random.Range(-0.5f, 0.5f));
                float timeToWait = animationClipTime + randomDelay;

                bool isTimeToHit = Time.time - lastHitTime > timeToWait;

                if(isTimeToHit)
                {
                    AttackTargetOnce();
                    lastHitTime = Time.time;
                }
                yield return new WaitForSeconds(timeToWait);
            }
            isAttacking = false;
        }


        private void AttackTargetOnce(float attackAdj = 0f, float damageAdj = 0f, float armourAvoidAdj = 0f, AnimationClip specialAttackAnimation = null)
        {
            if (targetHealthSystem == null || !targetIsAlive) { return; }            
            
            float damageDelay = currentWeaponConfig.GetDamageDelay();
            float damageDone = CalculateDamage(damageAdj, armourAvoidAdj, specialAttackAnimation);
            animator.SetTrigger(ATTACK_TRIGGER);
            if (TryToHit(attackAdj) == true)
            {
                StartCoroutine(DamageAfterDelay(damageDone, damageDelay));
            }
            else
            {
                StartCoroutine(HandleParryAfterDelay(damageDelay));
            }
        }

        private bool TryToHit(float attackAdj)
        {
            float attackScore = UnityEngine.Random.Range(1, 100) + attackBonus + attackAdj;

            float defenceBonus = target.GetComponent<WeaponSystem>().GetParryBonus();
            float adjustedDefenceBonus = AdjustForAttackDirection(defenceBonus);
            float defenceScore = UnityEngine.Random.Range(1, 100) + adjustedDefenceBonus;
            
            if(attackScore > defenceScore)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private float AdjustForAttackDirection(float defenceBonus)
        {
            Vector3 targetDir = transform.position - target.transform.position;
            float angleTargetAttackedFrom = Vector3.Angle(targetDir, target.transform.forward);
            float defencePenalty = 0f;

            if (angleTargetAttackedFrom <= 45)
            {
                defencePenalty = 0f;
            }
            else if (angleTargetAttackedFrom <= 135)
            {
                defencePenalty = globalCombatConfig.GetSideDefencePenalty;
            }
            else 
            {
                defencePenalty = globalCombatConfig.GetRearDefencePenalty;
            }

            float adjustedDefence = defenceBonus * (1 - defencePenalty);
            return adjustedDefence;
        }

        IEnumerator DamageAfterDelay (float damage, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            targetHealthSystem.AdjustHealth(damage);
        }

        IEnumerator HandleParryAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            var audioSource = GetComponent<AudioSource>();
            audioSource.volume = UnityEngine.Random.Range(0.5f, 1f);
            audioSource.PlayOneShot(currentWeaponConfig.GetParrySound());
        }

        private float CalculateDamage(float damageAdj, float armourAvoidAdj, AnimationClip specialAttackAnimation)
        {
            float damageDone = 0f;
            AnimationClip animationToSet; 
            ArmourSystem.ArmourProtection targetArmour = new ArmourSystem.ArmourProtection();
            ArmourSystem targetArmourSystem = target.GetComponent<ArmourSystem>();
            if (targetArmourSystem)
            {
                targetArmour = targetArmourSystem.CalculateArmour(armourAvoidAdj);
            }
            
            if (UnityEngine.Random.Range(0f, 1f) <= currentWeaponConfig.GetChanceForSwing())
            {
                animationToSet = currentWeaponConfig.GetSwingAnimClip();
                damageDone =  CalculateSwingDamage(targetArmour, damageAdj);
            }
            else
            {
                animationToSet = currentWeaponConfig.GetThrustAnimClip();
                damageDone = CalculateThrustDamage(targetArmour, damageAdj);
            }

            if (specialAttackAnimation)
            {
                animationToSet = specialAttackAnimation;
            }

            SetAttackAnimation(animationToSet);
            return damageDone;
        }

        private float CalculateSwingDamage(ArmourSystem.ArmourProtection targetArmour, float damageAdj)
        {
            float bluntDamageDone = (baseDamage+ damageAdj) * currentWeaponConfig.GetBluntDamageModification();
            float bluntDamageTaken = Mathf.Clamp(bluntDamageDone - targetArmour.blunt, 0f, bluntDamageDone);

            float bladeDamageDone = (baseDamage + damageAdj) * currentWeaponConfig.GetBladeDamageModification();
            float bladeDamageTaken = Mathf.Clamp(bladeDamageDone - targetArmour.blade, 0f, bladeDamageDone);

            // Debug.Log(Time.time + " Swing Dmg on " + target + ": " + bladeDamageTaken + " Blade, " + bluntDamageTaken + " Blunt." );
            return bluntDamageTaken + bladeDamageTaken;
        }
        
        private float CalculateThrustDamage(ArmourSystem.ArmourProtection targetArmour, float damageAdj)
        {
            float pierceDamageDone = (baseDamage + damageAdj) * currentWeaponConfig.GetPierceDamageModification();
            float pierceDamageTaken = Mathf.Clamp(pierceDamageDone - targetArmour.pierce, 0f, pierceDamageDone);
            // Debug.Log(Time.time + "Pierce Dmg on " + target + ": " + pierceDamageTaken);
            return pierceDamageTaken;
        }

        private void SetAttackAnimation(AnimationClip weaponAnimation)
        {
            if (!character.GetAnimatorOverrideController())
            {
                Debug.Break();
                Debug.LogAssertion("Please provide " + gameObject + " with an animator Override Controller");
            }
            else
            {
                var animatorOverrideController = character.GetAnimatorOverrideController();
                animator.runtimeAnimatorController = animatorOverrideController;
                animatorOverrideController[DEFAULT_ATTACK] = weaponAnimation;
            }
        }
    }
}
