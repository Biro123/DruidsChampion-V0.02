using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace RPG.Characters
{
    public class OffenceSystem : MonoBehaviour
    {
        [SerializeField] float attackBonus = 100f;
        [SerializeField] float baseDamage = 220f;
        [SerializeField] WeaponConfig currentWeaponConfig;

        DefenceSystem targetDefenceSystem;
        GlobalCombatConfig globalCombatConfig;

        GameObject target;
        HealthSystem targetHealthSystem;
        GameObject weaponObject;
        Animator animator;
        Character character;
        
        float lastHitTime;
        bool attackerIsAlive;
        bool targetIsAlive;
        bool targetInRange;
        bool isAttacking;
        
        const string ATTACK_TRIGGER = "Attack";
        const string DEFAULT_ATTACK = "DEFAULT ATTACK";
        const float ATTACK_RANGE_TOLERANCE = 0.5f;
        
        void Start()
        {
            animator = GetComponent<Animator>();
            character = GetComponent<Character>();

            globalCombatConfig = FindObjectOfType<GlobalCombatConfig>();
            if (!globalCombatConfig)
            {
                Debug.LogError("Global Combat Config missing from scene");
            }

            PutWeaponInHand(currentWeaponConfig);
            SetAttackAnimation(currentWeaponConfig.GetSwingAnimClip());   // TODO sets starting animation override - including movement
        }

        private void Update()
        {
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
            StopCoroutine("AttackTargetRepeatedly");
            animator.StopPlayback();
            isAttacking = false;
        }

        public void SetTarget(GameObject targetToSet) 
        {
            // Required to ensure that co-routines are not immediately stopped when AttackTarget is called.
            target = targetToSet;
            if (target)
            {
                targetHealthSystem = target.GetComponent<HealthSystem>();
                targetDefenceSystem = target.GetComponent<DefenceSystem>();
            }
            else
            {
                targetHealthSystem = null;
                targetDefenceSystem = null;
                StopAttacking();
            }
        }

        public void ChangeTarget(GameObject targetToSet)
        {
            if (target && target != targetToSet)
            {
                StopAllCoroutines();
                StartAttackingTarget(targetToSet);
            }
        }

        public void StartAttackingTarget(GameObject targetToAttack)
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
            if (GetComponent<PlayerControl>())
                print(Time.time + " starting attack repeatedly");
            isAttacking = true;
            if(startDelay != 0f)   
            {
                yield return new WaitForSeconds(startDelay);
            }

            while (attackerIsAlive && targetIsAlive && targetInRange)
            {
                AttackTargetOnce();

                var animationClip = currentWeaponConfig.GetSwingAnimClip();
                float animationClipTime = animationClip.length / character.GetAnimSpeedMultiplier();
                float randomDelay = currentWeaponConfig.GetTimeBetweenAnimationCycles();  // TODO re-add random
                    //* (1f + UnityEngine.Random.Range(-0.3f, 0.3f));
                float timeToWait = animationClipTime + randomDelay;

                if (GetComponent<PlayerControl>())
                    print(Time.time + " delay: " + timeToWait);

                yield return new WaitForSeconds(timeToWait);
            }
            isAttacking = false;
        }

        private void AttackTargetOnce(float attackAdj = 0f, float damageAdj = 0f, float armourAvoidAdj = 0f, AnimationClip specialAttackAnimation = null)
        {
            if (!targetHealthSystem || !targetDefenceSystem || !targetIsAlive) { return; }

            float bladeDamageDone, bluntDamageDone, pierceDamageDone;
            float attackScore = UnityEngine.Random.Range(1, 100) + attackBonus + attackAdj;

            if (UnityEngine.Random.Range(0f, 1f) <= currentWeaponConfig.GetChanceForSwing())
            {
                SetAttackAnimation( currentWeaponConfig.GetSwingAnimClip() );
                bluntDamageDone = (baseDamage + damageAdj) * currentWeaponConfig.GetBluntDamageModification();
                bladeDamageDone = (baseDamage + damageAdj) * currentWeaponConfig.GetBladeDamageModification();
                pierceDamageDone = 0f;
            }
            else
            {
                SetAttackAnimation( currentWeaponConfig.GetThrustAnimClip() );
                bluntDamageDone = 0f;
                bladeDamageDone = 0f;
                pierceDamageDone = (baseDamage + damageAdj) * currentWeaponConfig.GetPierceDamageModification();
            }
            if (specialAttackAnimation)
            {
                SetAttackAnimation(specialAttackAnimation);
            }

            animator.SetTrigger(ATTACK_TRIGGER);

            var displayDmgDone = bluntDamageDone + bladeDamageDone + pierceDamageDone;
            //print(Time.time + gameObject.name + " Attacks for " + displayDmgDone + " dmg delay " + currentWeaponConfig.GetDamageDelay());

            // Main call to Defence System
            targetDefenceSystem.DefendAgainstAttack(
                attackScore,
                FindDirectionDefencePenalty(),
                bluntDamageDone, 
                bladeDamageDone,
                pierceDamageDone,
                armourAvoidAdj,
                currentWeaponConfig.GetDamageDelay()
                );
            lastHitTime = Time.time;
        }        

        private float FindDirectionDefencePenalty()
        {
            Vector3 targetDir = transform.position - target.transform.position;
            float angleTargetAttackedFrom = Vector3.Angle(targetDir, target.transform.forward);

            if (angleTargetAttackedFrom <= 45)
            { return 0f; }
            else if (angleTargetAttackedFrom <= 135)
            { return globalCombatConfig.GetSideDefencePenalty; }
            else
            { return globalCombatConfig.GetRearDefencePenalty; }
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
