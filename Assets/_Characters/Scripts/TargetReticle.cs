using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    public class TargetReticle : MonoBehaviour
    {
        public void SetReticule(bool activeToSet)
        {
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer)
            {
                spriteRenderer.enabled = activeToSet;
            }
        }
    }
}
