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
                weaponSystem.StopAttacking();
                character.SetDestination(targetLocation);
                SetCurrentTarget(null);
            }
        }

        private void OnMouseOverEnemy(EnemyAI enemy)
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
                weaponSystem.SetTarget(enemy.gameObject);
                StartCoroutine(MoveAndAttack(enemy.gameObject));
            }

            if (Input.GetMouseButtonDown(1) && IsInRange(enemy.gameObject))
            {
                specialAbilities.AttemptSpecialAbility(0, enemy.gameObject);
            }
            else if (Input.GetMouseButtonDown(1) && !IsInRange(enemy.gameObject))
            {
                weaponSystem.SetTarget(enemy.gameObject);
                StartCoroutine(MoveAndSpecial(0, enemy.gameObject));
            }
        }

        private void SetCurrentTarget(EnemyAI enemy)
        {
            if (enemy)
            {
                currentTarget = enemy.gameObject;
                Debug.Log("current target = " + currentTarget.name);
                var targetReticule = currentTarget.GetComponentInChildren<TargetReticle>();
                if (targetReticule)
                {
                    targetReticule.SetReticule(true);
                }
            }

            if (!enemy && currentTarget )
            {
                var targetReticule = currentTarget.GetComponentInChildren<TargetReticle>();
                if (targetReticule)
                {
                    targetReticule.SetReticule(false);
                }
                currentTarget = null;
            }
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
        }
              
        private bool IsInRange(GameObject target)
        {
            float distanceToTarget = (target.transform.position - this.transform.position).magnitude;
            return distanceToTarget <= weaponSystem.GetCurrentWeapon().GetAttackRange();
        }        
    }
}
