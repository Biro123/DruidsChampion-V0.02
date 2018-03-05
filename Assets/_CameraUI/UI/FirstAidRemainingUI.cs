using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using RPG.Characters;

namespace RPG.CameraUI
{
    public class FirstAidRemainingUI : MonoBehaviour
    {
        private PlayerControl player;
        private FirstAidBehaviour firstAidBehaviour;
        Text text;

        // Use this for initialization
        void Start()
        {
            player = FindObjectOfType<PlayerControl>();            
            text = GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            firstAidBehaviour = player.gameObject.GetComponent<FirstAidBehaviour>();
            if (firstAidBehaviour)
            {
                text.text = firstAidBehaviour.GetRemainingUses().ToString();
            }
        }
    }
}
