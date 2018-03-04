using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [ExecuteInEditMode]  // runs in edit-mode as well as runtime
    public class WeaponPickup : MonoBehaviour
    {        
        [SerializeField] WeaponConfig weaponConfig = null;
        [SerializeField] AudioClip pickupSFX = null;

        AudioSource audioSource = null;
        bool isPickedUp;

        // Use this for initialization
        void Start()
        {
            isPickedUp = false;
            audioSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!Application.isPlaying)  // only do this while in editor
            {
                DestroyChildren();
                InstantiateWeapon();
            }
        }

        private void DestroyChildren()
        {
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        private void InstantiateWeapon()
        {
            var weapon = weaponConfig.GetWeaponPrefab();
            weapon.transform.position = Vector3.zero;
            Instantiate(weapon, gameObject.transform);            
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerControl>() && !isPickedUp)
            {
                isPickedUp = true;
                other.GetComponent<OffenceSystem>().PutWeaponInHand(weaponConfig);
                audioSource.clip = pickupSFX;
                audioSource.Play();
                Destroy(gameObject, audioSource.clip.length);
            }
        }

    }
}
