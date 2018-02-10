using System;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Characters
{
    [SelectionBase]
    public class Character : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] RuntimeAnimatorController animatorController;
        [SerializeField] AnimatorOverrideController animatorOverrideController;
        [SerializeField] Avatar characterAvatar;
        [SerializeField] [Range(0.1f, 1f)] float walkOrRun = 1f;

        [Header("Audio")]
        [SerializeField] [Range(0f, 1f)] float audioVolume = 1;

        [Header("Capsule Collider")]
        [SerializeField] Vector3 colliderCentre = new Vector3(0, 0.9f, 0f);
        [SerializeField] float colliderRadius = 0.8f;
        [SerializeField] float colliderHeight = 1.8f;

        [Header("Movement")]
        [SerializeField] float moveSpeedMultiplier = 1f;
        [SerializeField] float animationSpeedMultiplier = 1f;
        [SerializeField] float movingTurnSpeed = 360;
        [SerializeField] float stationaryTurnSpeed = 180;

        [Header("Navigation")]
        [SerializeField] float navMeshAgentSteeringSpeed = 1.2f;
        [SerializeField] float navMeshAgentStoppingDistance = 0.2f;
        [SerializeField] bool navMeshAgentAutoBraking = true;

        [Header("Rigid Body")]
        [SerializeField] float rigidBodyMass = 80f;

        NavMeshAgent navMeshAgent;
        Animator animator;
        Rigidbody rigidBody;
        float turnAmount;
        float forwardAmount;
        bool isAlive = true;

        private void Awake()
        {
            AddRequiredComponents();
        }

        private void AddRequiredComponents()
        {
            animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            animator.avatar = characterAvatar;

            var capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.center = colliderCentre;
            capsuleCollider.radius = colliderRadius;
            capsuleCollider.height = colliderHeight;

            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidBody.mass = rigidBodyMass;

            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = audioVolume;

            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = false;
            navMeshAgent.autoBraking = navMeshAgentAutoBraking;
            navMeshAgent.stoppingDistance = navMeshAgentStoppingDistance;
            navMeshAgent.speed = navMeshAgentSteeringSpeed;
        }

        private void Update()
        {
            if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance && isAlive)            {
                Move(navMeshAgent.desiredVelocity);                
            }
            else
            {
                Move(Vector3.zero);
            }
        }

        public float GetAnimSpeedMultiplier()
        {
            return animationSpeedMultiplier;
        }

        public void Kill()
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.radius = 0.1f;
            var collider = GetComponent<CapsuleCollider>();
            if (collider)
            {
                collider.enabled = false;
            }
            isAlive = false;
        }

        public void SetDestination(Vector3 worldPosition)
        {
            navMeshAgent.destination = worldPosition; 
        }

        private void Move(Vector3 movement)
        {
            SetForwardAndTurn(movement);
            ApplyExtraTurnRotation();
            UpdateAnimator();
        }

        public AnimatorOverrideController GetAnimatorOverrideController()
        {
            var weapon = GetComponent<WeaponSystem>().GetCurrentWeapon();
            var weaponSpecificAnimationOverrides = weapon.GetWeaponSpecificAnimations();
            if(weaponSpecificAnimationOverrides)
            {
                return weaponSpecificAnimationOverrides;
            }
            else
            {
                return animatorOverrideController;
            }
            
        }

        private void SetForwardAndTurn(Vector3 movement)
        {
            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired direction.
            if (movement.magnitude > 1f)
            {
                movement.Normalize();
            }
            var localMove = transform.InverseTransformDirection(movement);
            //   move = Vector3.ProjectOnPlane(move, m_GroundNormal);  Was this removal ok?
            turnAmount = Mathf.Atan2(localMove.x, localMove.z);
            forwardAmount = localMove.z;
        }

        void ApplyExtraTurnRotation()
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
            transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
        }

        void UpdateAnimator()
        {
            animator.SetFloat("Forward", forwardAmount * walkOrRun, 0.1f, Time.deltaTime);
            animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
            animator.speed = animationSpeedMultiplier;
        }

        public void OnAnimatorMove()
        {
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (Time.deltaTime > 0 )
            {                
                Vector3 velocity = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;

                // we preserve the existing y part of the current velocity.
                velocity.y = rigidBody.velocity.y;
                rigidBody.velocity = velocity;
            }
        }
    }
}
