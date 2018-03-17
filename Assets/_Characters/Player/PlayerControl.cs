using UnityEngine;
using System.Collections;
using RPG.CameraUI;    // For mouse events  
using System;

namespace RPG.Characters
{
    public class PlayerControl : MonoBehaviour
    {
        [SerializeField] float targetClickTolerance = 2f;

        SpecialAbilities specialAbilities;
        Character character;
        OffenceSystem weaponSystem;
        GameObject currentTarget;
        bool isStrafing = false;
        int opponentLayerMask = 0;

        const int COMBATANT_LAYER = 9;

        public void MoveAndKick(Vector3 moveToPosition, GameObject kickObject)
        {
            character.SetDestination(moveToPosition);
            StartCoroutine(KickWhenNear(moveToPosition, kickObject));
        }

        IEnumerator KickWhenNear(Vector3 moveToPosition, GameObject kickObject)
        {
            while (Vector3.Distance(transform.position, moveToPosition) > 1f)
            {
                yield return new WaitForEndOfFrame();
            }
            character.SetDestination(transform.position);
            transform.LookAt(kickObject.transform.position);
            GetComponent<Animator>().SetTrigger("Kick");
            kickObject.GetComponent<Barrier>().DestroySelf();
            yield return new WaitForEndOfFrame();
        }

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
            if (!isActiveAndEnabled) { return; }

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
            if (Input.GetKeyDown(KeyCode.H))
            {
                specialAbilities.AttemptSpecialAbility(3);  // TODO handle keybinds better
                return;
            }

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
            if(!isActiveAndEnabled) { return; }

            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                Transform nearestTargetToClick = FindTargetInRange(targetLocation, targetClickTolerance);
                if (nearestTargetToClick) // click on ground close to target
                {
                    HandleActionOnEnemy(nearestTargetToClick.GetComponent<CombatantAI>());
                }
                else
                {
                    if (Input.GetMouseButton(0)) // click on ground
                    {
                        StopAllCoroutines();
                        SetCurrentTarget(null);
                        character.SetDestination(targetLocation);
                    }
                }
            }
        } 

        private void OnMouseOverEnemy(CombatantAI enemy)
        {
            if (isActiveAndEnabled)
            {
                HandleActionOnEnemy(enemy);
            }
        }

        private void HandleActionOnEnemy(CombatantAI enemy)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                SetCurrentTarget(enemy);
            }

            if (Input.GetMouseButton(0) && IsInRange(enemy.gameObject))
            {
                weaponSystem.StartAttackingTarget(enemy.gameObject);
            }
            else if (Input.GetMouseButton(0) && !IsInRange(enemy.gameObject))
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

        private Transform FindTargetInRange(Vector3 clickLocation, float aggroRange)
        {
            // See what are in range
            Collider[] opponentsInRange = Physics.OverlapSphere(clickLocation, aggroRange, opponentLayerMask);
            if (opponentsInRange.Length == 0) { return null; }

            // Find closest in range
            float closestRange = 0;
            Collider closestTarget = null;
            foreach (var opponentInRange in opponentsInRange)
            {
                var opponentCombatantAI = opponentInRange.GetComponent<CombatantAI>();
                if (opponentCombatantAI && opponentCombatantAI.GetIsEnemy())
                {
                    float currentRange = (clickLocation - opponentInRange.transform.position).magnitude;
                    if (closestTarget == null || currentRange < closestRange)
                    {
                        closestTarget = opponentInRange;
                        closestRange = currentRange;
                    }
                }
            }
            if (closestTarget)
            { return closestTarget.transform; }
            else
            { return null; }
        }
    }
}
