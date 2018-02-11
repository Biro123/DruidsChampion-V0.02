using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Characters
{
    public class GlobalCombatConfig : MonoBehaviour
    {
        [SerializeField] [Range(0f, 1f)] float sideDefencePenalty = 0.3f;  //percentage
        [SerializeField] [Range(0f, 1f)] float rearDefencePenalty = 0.5f;  //percentage

        public float GetSideDefencePenalty
        {
            get { return sideDefencePenalty; }
        }

        public float GetRearDefencePenalty
        {
            get { return rearDefencePenalty; }
        }

    }
}
