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
        CombatantAI combatantAI;

        // Weapon Stats
        float attackRange;
        float damageDelay;
        float timeBetweenAttacks;
        float chanceForSwing;
        float bluntDamageModification;
        float bladeDamageModification;
        float pierceDamageModification;

        AnimatorOverrideController animatorOverrideController;
        
        bool attackerIsAlive;
        bool targetIsAlive;
        bool targetInRange;
        bool isAttacking;
        
        const string SWING_ATTACK_TRIGGER = "SwingAttack";
        const string THRUST_ATTACK_TRIGGER = "ThrustAttack";
        const string SPECIAL_ABILITY_TRIGGER = "SpecialAbility";
        const string DEFAULT_SWING_ATTACK = "DEFAULT SWING ATTACK";
        const string DEFAULT_THRUST_ATTACK = "DEFAULT THRUST ATTACK";
        const string SPECIAL_ABILITY = "DEFAULT SPECIAL ABILITY";
        const float ATTACK_RANGE_TOLERANCE = 0.5f;
        
        void Start()
        {
            animator = GetComponent<Animator>();
            character = GetComponent<Character>();
            combatantAI = GetComponent<CombatantAI>();

            globalCombatConfig = FindObjectOfType<GlobalCombatConfig>();
            if (!globalCombatConfig)
            {
                Debug.LogError("Global Combat Config missing from scene");
            }

            PutWeaponInHand(currentWeaponConfig);                     
        }

        private void Update()
        {
            SetAttackerAndTargetState();

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

        private void SetAttackerAndTargetState()
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
                targetInRange = (distanceToTarget <= attackRange + ATTACK_RANGE_TOLERANCE);
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
            GetWeaponStats(currentWeaponConfig);
            SetWeaponAnimations();
        }

        private void SetWeaponAnimations()
        {
            if (!character.GetAnimatorOverrideController())
            {
                Debug.Break();
                Debug.LogAssertion("Please provide " + gameObject + " with an animator Override Controller");
            }
            else
            {
                animatorOverrideController = character.GetAnimatorOverrideController();
                animator.runtimeAnimatorController = animatorOverrideController;
                // Remove hit events
                if (animatorOverrideController[DEFAULT_THRUST_ATTACK])
                {
                    animatorOverrideController[DEFAULT_THRUST_ATTACK].events = new AnimationEvent[0];
                }
                if (animatorOverrideController[DEFAULT_SWING_ATTACK])
                {
                    animatorOverrideController[DEFAULT_SWING_ATTACK].events = new AnimationEvent[0];
                }
            }
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

        private void GetWeaponStats(WeaponConfig weaponConfig)
        {
            attackRange = weaponConfig.GetAttackRange();
            damageDelay = weaponConfig.GetDamageDelay();
            timeBetweenAttacks = weaponConfig.GetTimeBetweenAttacks();
            chanceForSwing = weaponConfig.GetChanceForSwing();
            bluntDamageModification = weaponConfig.GetBluntDamageModification();
            bladeDamageModification = weaponConfig.GetBladeDamageModification();
            pierceDamageModification = weaponConfig.GetPierceDamageModification();
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
            SetAttackerAndTargetState();
        }

        public void ChangeTarget(GameObject targetToSet)
        {
            if (target && target != targetToSet)
            {
                isAttacking = false;
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
                if (combatantAI && combatantAI.GetIsEnemy()) // slightly delay start of enemy attacking
                {
                    startDelay = damageDelay;
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

            while (attackerIsAlive && targetIsAlive && targetInRange)
            {
                AttackTargetOnce();                                

                float randomDelay = timeBetweenAttacks  
                    * (1f + UnityEngine.Random.Range(-0.2f, 0.2f));

                yield return new WaitForSeconds(randomDelay);
            }
            isAttacking = false;
        }

        private void AttackTargetOnce(float attackAdj = 0f, float damageAdj = 0f, float armourAvoidAdj = 0f, AnimationClip specialAttackAnimation = null)
        {
            if (!targetHealthSystem || !targetDefenceSystem || !targetIsAlive) { return; }

            float bladeDamageDone, bluntDamageDone, pierceDamageDone;
            float attackScore = UnityEngine.Random.Range(1, 100) + attackBonus + attackAdj;
            string animationToTrigger;

            if (UnityEngine.Random.Range(0f, 1f) <= chanceForSwing)
            {
                bluntDamageDone = (baseDamage + damageAdj) * bluntDamageModification;
                bladeDamageDone = (baseDamage + damageAdj) * bladeDamageModification;
                pierceDamageDone = 0f;
                animationToTrigger = SWING_ATTACK_TRIGGER;
            }
            else
            {
                bluntDamageDone = 0f;
                bladeDamageDone = 0f;
                pierceDamageDone = (baseDamage + damageAdj) * pierceDamageModification;
                animationToTrigger = THRUST_ATTACK_TRIGGER;
            }
            if (specialAttackAnimation)
            {
                animatorOverrideController[SPECIAL_ABILITY] = specialAttackAnimation;
                animationToTrigger = SPECIAL_ABILITY_TRIGGER;
            }
            animator.SetTrigger(animationToTrigger);

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
                damageDelay
                );
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
    }
}
