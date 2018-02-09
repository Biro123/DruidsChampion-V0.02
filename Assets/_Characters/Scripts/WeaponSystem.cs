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

        float lastHitTime;
        bool attackerIsAlive;
        bool targetIsAlive;

        const string ATTACK_TRIGGER = "Attack";
        const string DEFAULT_ATTACK = "DEFAULT ATTACK";

        public float GetParryBonus() {  return parryBonus; }

        void Start()
        {
            animator = GetComponent<Animator>();
            character = GetComponent<Character>();

            PutWeaponInHand(currentWeaponConfig);
            SetAttackAnimation(currentWeaponConfig.GetSwingAnimClip());
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
                targetInRange = (distanceToTarget <= currentWeaponConfig.GetAttackRange());
            }

            if (targetIsAlive && attackerIsAlive && targetInRange)
            {
                //FaceTarget();
                transform.LookAt(target.transform);
            }
            else
            {
                StopAllCoroutines();
            }
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

        public void StopAttacking()
        {
            animator.StopPlayback();
            StopAllCoroutines();
        }

        public void SetTarget(GameObject targetToSet) 
        {
            // Required to ensure that co-routines are not immediately stopped when AttackTarget is called.
            target = targetToSet;
            targetHealthSystem = target.GetComponent<HealthSystem>();
        }

        public void AttackTarget(GameObject targetToAttack)
        {
            SetTarget(targetToAttack);         
            StartCoroutine(AttackTargetRepeatedly());
        }

        public void SpecialAttack(GameObject targetToAttack, float attackAdj, float damageAdj, float armourAvoidAdj)
        {
            SetTarget(targetToAttack);
            AttackTargetOnce(attackAdj, damageAdj, armourAvoidAdj);
        }

        IEnumerator AttackTargetRepeatedly()
        {
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
        }

        public WeaponConfig GetCurrentWeapon()
        {
            return currentWeaponConfig;
        }
        
        private void SetAttackAnimation(AnimationClip weaponAnimation)
        {
            if (!character.GetAnimatorOverrideController())
            {
                Debug.Break();
                Debug.LogAssertion("Please proved " + gameObject + " with an animator Override Controller");
            }
            else
            {
                var animatorOverrideController = character.GetAnimatorOverrideController();
                animator.runtimeAnimatorController = animatorOverrideController;
                animatorOverrideController[DEFAULT_ATTACK] = weaponAnimation; 
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

        private void AttackTargetOnce(float attackAdj = 0f, float damageAdj = 0f, float armourAvoidAdj = 0f)
        {
            if (targetHealthSystem == null) { return; }            
            
            float damageDelay = currentWeaponConfig.GetDamageDelay();
            float damageDone = CalculateDamage(damageAdj, armourAvoidAdj);
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
            float defenceScore = UnityEngine.Random.Range(1, 100) + target.GetComponent<WeaponSystem>().GetParryBonus();

            if(attackScore > defenceScore)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            audioSource.PlayOneShot(currentWeaponConfig.GetParrySound());
        }

        private void FaceTarget()  // currently not used
        {
            var attackTurnSpeed = character.GetAttackTurnRate();
            var amountToRotate = Quaternion.LookRotation(target.transform.position - this.transform.position);
            var rotateSpeed = attackTurnSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, amountToRotate, rotateSpeed);
        }

        private float CalculateDamage(float damageAdj, float armourAvoidAdj)
        {
            ArmourSystem.ArmourProtection targetArmour = new ArmourSystem.ArmourProtection();
            ArmourSystem targetArmourSystem = target.GetComponent<ArmourSystem>();
            if (targetArmourSystem)
            {
                targetArmour = targetArmourSystem.CalculateArmour(armourAvoidAdj);
            }

            if (UnityEngine.Random.Range(0f, 1f) <= currentWeaponConfig.GetChanceForSwing())
            {
                SetAttackAnimation(currentWeaponConfig.GetSwingAnimClip());
                return CalculateSwingDamage(targetArmour, damageAdj);
            }
            else
            {
                SetAttackAnimation(currentWeaponConfig.GetThrustAnimClip());
                return CalculateThrustDamage(targetArmour, damageAdj);
            }
        }

        private float CalculateSwingDamage(ArmourSystem.ArmourProtection targetArmour, float damageAdj)
        {
            float bluntDamageDone = (baseDamage+ damageAdj) * currentWeaponConfig.GetBluntDamageModification();
            float bluntDamageTaken = Mathf.Clamp(bluntDamageDone - targetArmour.blunt, 0f, bluntDamageDone);

            float bladeDamageDone = (baseDamage + damageAdj) * currentWeaponConfig.GetBladeDamageModification();
            float bladeDamageTaken = Mathf.Clamp(bladeDamageDone - targetArmour.blade, 0f, bladeDamageDone);

            Debug.Log(Time.time + " Swing Dmg on " + target + ": " + bladeDamageTaken + " Blade, " + bluntDamageTaken + " Blunt." );
            return bluntDamageTaken + bladeDamageTaken;
        }
        
        private float CalculateThrustDamage(ArmourSystem.ArmourProtection targetArmour, float damageAdj)
        {
            float pierceDamageDone = (baseDamage + damageAdj) * currentWeaponConfig.GetPierceDamageModification();
            float pierceDamageTaken = Mathf.Clamp(pierceDamageDone - targetArmour.pierce, 0f, pierceDamageDone);
            Debug.Log(Time.time + "Pierce Dmg on " + target + ": " + pierceDamageTaken);
            return pierceDamageTaken;
        }
    }
}
