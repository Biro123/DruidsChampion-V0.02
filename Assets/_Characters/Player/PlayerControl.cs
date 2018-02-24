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
        WeaponSystem weaponSystem;
        GameObject currentTarget;
        bool isStrafing = false;
        
        private void Start()
        {
            character = GetComponent<Character>();
            specialAbilities = GetComponent<SpecialAbilities>();
            weaponSystem = GetComponent<WeaponSystem>();

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
                        weaponSystem.AttackTarget(currentTarget);
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
                weaponSystem.AttackTarget(enemy.gameObject);
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
            if (targetToSet)
            {
                currentTarget = targetToSet.gameObject;
                var targetReticule = currentTarget.GetComponentInChildren<TargetReticle>();
                if (targetReticule)
                {
                    targetReticule.SetReticule(true);
                }
            }

            if (!targetToSet && currentTarget )
            {
                var targetReticule = currentTarget.GetComponentInChildren<TargetReticle>();
                if (targetReticule)
                {
                    targetReticule.SetReticule(false);
                }
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
            weaponSystem.AttackTarget(target.gameObject);
        }

        IEnumerator MoveAndSpecial (int specialAbilityIndex, GameObject target)
        {
            yield return StartCoroutine(MoveToTarget(target));
            specialAbilities.AttemptSpecialAbility(specialAbilityIndex, target.gameObject);
            weaponSystem.AttackTarget(target.gameObject);
        }
              
        private bool IsInRange(GameObject target)
        {
            float distanceToTarget = (target.transform.position - this.transform.position).magnitude;
            return distanceToTarget <= weaponSystem.GetCurrentWeapon().GetAttackRange();
        }        
    }
}
