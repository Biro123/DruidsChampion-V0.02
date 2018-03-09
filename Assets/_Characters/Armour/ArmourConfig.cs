using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{

    [CreateAssetMenu(menuName = ("RPG/Armour"))]
    public class ArmourConfig : ScriptableObject
    {
        [Range(0.1f, 1.2f)] [SerializeField] float quality = 0.8f;
        [Range(0.1f, 1.0f)] [SerializeField] float condition = 0.8f;

        [Range(0.0f, 1.0f)] [SerializeField] float armourCoverage = 0.4f;
        [SerializeField] float bladeArmourAmount = 20f;
        [SerializeField] float bluntArmourAmount = 20f;
        [SerializeField] float pierceArmourAmount = 20f;

        public float GetArmourCoverage() { return armourCoverage * condition; }
        public float GetBladeArmourAmount() { return bladeArmourAmount * quality * condition; }
        public float GetBluntArmourAmount() { return bluntArmourAmount * quality * condition; }
        public float GetPierceArmourAmount() { return pierceArmourAmount * quality * condition; }

    }
}
