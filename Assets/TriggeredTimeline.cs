using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using RPG.Characters;

namespace RPG.CameraUI
{
    public class TriggeredTimeline : MonoBehaviour
    {
        PlayableDirector timeline;
        bool inTriggerArea;
        GameObject playerInArea;

        void Start()
        {
            timeline = GetComponent<PlayableDirector>();
        }

        private void Update()
        {
            if (playerInArea)
            {
                if (timeline.state == PlayState.Playing)
                {
                    playerInArea.GetComponent<PlayerControl>().enabled = false;
                }
                else
                {
                    playerInArea.GetComponent<PlayerControl>().enabled = true;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            print("Timeline Trigger enter " + timeline.name);
            if (other.gameObject.tag == "Player")
            {
                timeline.Play();
                playerInArea = other.gameObject;
                playerInArea.GetComponent<Character>().SetDestination(transform.position);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                timeline.Stop();
                playerInArea = null;
            }
        }
    }
}
