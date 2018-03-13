using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using RPG.Characters;

namespace RPG.CameraUI
{
    public class TriggeredTimeline : MonoBehaviour
    {
        [SerializeField] bool isRepeatable = false;

        PlayableDirector timeline;
        bool inTriggerArea;
        bool hasTriggered = false;
        GameObject playerInArea;

        void Start()
        {
            timeline = GetComponent<PlayableDirector>();
        }

        private void Update()
        {
            if (playerInArea && timeline.state != PlayState.Playing)
            {
                playerInArea.GetComponent<PlayerControl>().enabled = true;
                playerInArea = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<PlayerControl>())
            {
                if (isRepeatable || !hasTriggered)
                {
                    hasTriggered = true;
                    timeline.Play();
                    playerInArea = other.gameObject;
                    playerInArea.GetComponent<Character>().SetDestination(transform.position);
                    playerInArea.GetComponent<PlayerControl>().enabled = false;
                }
            }
        }
    }
}
