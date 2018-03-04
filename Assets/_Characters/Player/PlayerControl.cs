using UnityEngine;
using System.Collections;
using RPG.CameraUI;    // For mouse events  
using System;

namespace RPG.Characters
{
    public class PlayerControl : MonoBehaviour
    {
        SpecialAbilities specialAbilities;
        Character character;
        OffenceSystem weaponSystem;
        GameObject currentTarget;
        bool isStrafing = false;
        int opponentLayerMask = 0;

        const int COMBATANT_LAYER = 9;

        private void Start()
        {
            character = GetComponent<Character>();
            specialAbilities = GetComponent<SpecialAbilities>();
            weaponSystem = GetComponent<OffenceSystem>();

            opponentLayerMask = opponentLayerMask | (1 << COMBATANT_LAYER);
            RegisterForMouseEvents();
        }

        private void RegisterForMouseEvents()
        {
            // Subscribe to Raycaster's on click event.
            var cameraRaycaster = Camera.main.GetComponent<CameraRaycaster>();
            cameraRaycaster.onMouseOverEnemy += OnMouseOverEnemy;
            cameraRaycaster.onMouseOverWalkable += OnMouseOverWalkable;
        }

        private void Update()
        {
            ScanForAbilityKeyDown();
            ScanForStrafeKey();
        }

        private void ScanForStrafeKey()
        {
            // GetKey is used rather than GetKeyDown to register while attack animation is underway 
            if (Input.GetKey(KeyCode.A) && currentTarget && IsInRange(currentTarget))
            {
                isStrafing = true;
                weaponSystem.StopAttacking();
                character.StrafeLeft(true);
                character.StrafeRight(false);
            }
            else if (Input.GetKey(KeyCode.D) && currentTarget && IsInRange(currentTarget))
            {
                isStrafing = true;
                weaponSystem.StopAttacking();
                character.StrafeRight(true);
                character.StrafeLeft(false);
            }
            else
            {
                if (isStrafing)
                {
                    character.StrafeRight(false);
                    character.StrafeLeft(false);
                    isStrafing = false;
                    if (currentTarget && IsInRange(currentTarget))
                    {
                        weaponSystem.StartAttackingTarget(currentTarget);
                    }
                }
            }
        }

        private void ScanForAbilityKeyDown()
        {
            for (int keyIndex = 1; keyIndex <= specialAbilities.GetNumberOfAbilities(); keyIndex++)
            {
                if (Input.GetKeyDown(keyIndex.ToString()))
                {
                    specialAbilities.AttemptSpecialAbility(keyIndex - 1);
                }
            }
        }

        private void OnMouseOverWalkable(Vector3 targetLocation)
        {
            if (Input.GetMouseButton(0))
            {
                StopAllCoroutines();
                SetCurrentTarget(null);
                character.SetDestination(targetLocation);                
            }
        }

        private void OnMouseOverEnemy(CombatantAI enemy)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                SetCurrentTarget(enemy);
            }

            if (Input.GetMouseButton(0) && IsInRange(enemy.gameObject))
            {
                weaponSystem.StartAttackingTarget(enemy.gameObject);
            }
            else if (Input.GetMouseButton(0) && ! IsInRange(enemy.gameObject))
            {
                StartCoroutine(MoveAndAttack(enemy.gameObject));
            }

            if (Input.GetMouseButtonDown(1) && IsInRange(enemy.gameObject))
            {
                specialAbilities.AttemptSpecialAbility(0, enemy.gameObject);
            }
            else if (Input.GetMouseButtonDown(1) && !IsInRange(enemy.gameObject))
            {
                StartCoroutine(MoveAndSpecial(0, enemy.gameObject));
            }
        }

        private void SetCurrentTarget(CombatantAI targetToSet)
        {
            if (currentTarget && (!targetToSet || targetToSet.GetInstanceID() != currentTarget.GetInstanceID() ) )
            {
                var targetReticule = currentTarget.GetComponentInChildren<TargetReticle>();
                if (targetReticule)
                {
                    targetReticule.SetReticule(false);
                }
            }

            if (targetToSet)
            {
                var targetReticule = targetToSet.GetComponentInChildren<TargetReticle>();
                if (targetReticule)
                {
                    targetReticule.SetReticule(true);
                }
                currentTarget = targetToSet.gameObject;
            }
            else
            {
                currentTarget = null;
            }

            weaponSystem.SetTarget(currentTarget);
        }

        IEnumerator MoveToTarget(GameObject target)
        {
            character.SetDestination(target.transform.position);
            while (!IsInRange(target))  
            {
                character.SetDestination(target.transform.position);
                yield return new WaitForEndOfFrame();
            }
            character.SetDestination(transform.position);
            yield return new WaitForEndOfFrame();
        }

        IEnumerator MoveAndAttack (GameObject target)
        {
            yield return StartCoroutine(MoveToTarget(target));
            weaponSystem.StartAttackingTarget(target.gameObject);
        }

        IEnumerator MoveAndSpecial (int specialAbilityIndex, GameObject target)
        {
            yield return StartCoroutine(MoveToTarget(target));
            specialAbilities.AttemptSpecialAbility(specialAbilityIndex, target.gameObject);
            weaponSystem.StartAttackingTarget(target.gameObject);
        }
              
        private bool IsInRange(GameObject target)
        {
            float distanceToTarget = (target.transform.position - this.transform.position).magnitude;
            return distanceToTarget <= weaponSystem.GetCurrentWeapon().GetAttackRange();
        }

        public bool IsSafeDistance(float safeRange)
        {
            // See what are in range
            Collider[] opponentsInRange = Physics.OverlapSphere(this.transform.position, safeRange, opponentLayerMask);
            if (opponentsInRange.Length == 0) { return true; }

            foreach (var opponentInRange in opponentsInRange)
            {
                CombatantAI opponentCombatantAI = opponentInRange.GetComponent<CombatantAI>();
                if (opponentCombatantAI && opponentCombatantAI.GetIsEnemy())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
