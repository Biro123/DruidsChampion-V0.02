using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Characters;

namespace RPG.CameraUI
{
    public class FaderRaycaster : MonoBehaviour
    {
        const int FADABLE_LAYER = 20;

        private PlayerControl player;   
        private int fadeLayerMask = 0;  
        float maxRaycastDepth = 100f;

        // Use this for initialization
        void Start()
        {
            player = FindObjectOfType<PlayerControl>();

            // This line shifts a binary bit of 1 left (int)layer times and
            // does a '|' (binary OR) to merge the bits with the previous - so for each bit,
            // if either or both a '1', the result is a '1'
            fadeLayerMask = fadeLayerMask | (1 << FADABLE_LAYER);            
        }

        // Update is called once per frame
        void Update()
        {
            FindBlockingObject();
        }

        private void FindBlockingObject()
        {
            // Finds objects blocking LoS from Camera to Player

            // Define the Ray to cast - from camera to player
            Ray ray = new Ray(transform.position, player.transform.position - transform.position);
            Debug.DrawRay(transform.position, player.transform.position - transform.position);

            RaycastHit[] hits;
            // the ~ in front of notFadePlayerMask is a binary NOT
            hits = Physics.RaycastAll(ray, maxRaycastDepth, fadeLayerMask);
            foreach (RaycastHit hit in hits)
            {
                HandleFade(hit);
            }
        }

        private static void HandleFade(RaycastHit hit)
        {
            Renderer hitRenderer = hit.transform.gameObject.GetComponent<Renderer>();

            if (hitRenderer == null) { return; } // skip if no renderer present

            Fader fader = hitRenderer.GetComponent<Fader>();
            if (fader == null) // fader script not attached to object hit
            {
                fader = hitRenderer.gameObject.AddComponent<Fader>();
            }
            fader.BeTransparent();
        }

    }
}
