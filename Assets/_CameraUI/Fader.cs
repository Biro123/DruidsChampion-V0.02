using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.CameraUI
{
    public class Fader : MonoBehaviour
    {

        private bool shouldBeTransparent;
        private float transparency;
        private float targetTransparency = 0.3f;
        private float fallOff = 0.3f;  // time to fade
        private Shader oldShader;
        private Color oldColor;
        private Material material;

        // Use this for initialization
        void Start()
        {
            material = GetComponent<Renderer>().material;
            if (material)
            {
                // Save the current shader
                oldShader = material.shader;
                material.shader = Shader.Find("Transparent/Diffuse");
                if (material.HasProperty("_Color"))
                {
                    oldColor = material.color;
                    transparency = oldColor.a;
                }
                else
                {
                    oldColor = Color.white;
                    transparency = 1.0f;
                }
            }
            else
            {
                transparency = 1.0f;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Visible again - so destroy.
            if (!shouldBeTransparent && transparency >= 1.0)
            {
                Destroy(this);
            }

            if (shouldBeTransparent)  // Fading Out
            {
                if (transparency >= targetTransparency)
                {
                    transparency -= ((1.0f - targetTransparency) * Time.deltaTime) / fallOff;
                }
            }
            else  // Fading In
            {
                transparency += ((1.0f - targetTransparency) * Time.deltaTime) / fallOff;
            }

            Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, transparency);
            material.SetColor("_Color", newColor);
            shouldBeTransparent = false;
        }

        public void BeTransparent()
        {
            shouldBeTransparent = true;
        }

        private void OnDestroy()
        {
            if (!oldShader) { return; }
            // Reset the shader
            material.shader = oldShader;
            material.color = oldColor;
        }

    }
}
