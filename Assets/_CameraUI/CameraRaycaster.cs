using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using RPG.Characters; 
using System;

namespace RPG.CameraUI
{
    public class CameraRaycaster : MonoBehaviour
    {
        [SerializeField] Texture2D walkCursor = null;
        [SerializeField] Texture2D targetCursor = null;
        [SerializeField] Vector2 cursorHotspot = new Vector2(4, 4);

        const int WALKABLE_LAYER = 8;
        float maxRaycastDepth = 100f;  // Hard coded value

        // Delegates allow other class to 'Subscribe' to them
        public delegate void OnMouseOverEnemy(EnemyAI enemy);     // Declare new delegate type
        public event OnMouseOverEnemy onMouseOverEnemy;         // Instantiate an observer set

        public delegate void OnMouseOverTerrain(Vector3 destination);
        public event OnMouseOverTerrain onMouseOverWalkable;


        void Update()
        {
            // Check if pointer is over an interactable UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // TODO Implement UI Imteraction
            }
            else
            {
                PerformRaycasts();
            }
        }

        void PerformRaycasts()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Specify Layer Priorities Here
            if ( RaycastForEnemy(ray) )    { return; }
            if ( RaycastForWalkable(ray) ) { return; }
        }
        
        private bool RaycastForEnemy(Ray ray)
        {
            RaycastHit hitInfo;
            bool hitAnything = Physics.Raycast(ray, out hitInfo, maxRaycastDepth);
            if (!hitAnything) { return false; } 

            GameObject gameObjectHit = hitInfo.collider.gameObject;
            EnemyAI enemyHit = gameObjectHit.GetComponent<EnemyAI>();
            if (enemyHit)
            {
                Cursor.SetCursor(targetCursor, cursorHotspot, CursorMode.Auto);
                onMouseOverEnemy(enemyHit);  // Broadcast Mouse over Enemy   
                return true;
            }                
            return false;
        }

        private bool RaycastForWalkable(Ray ray)
        {
            // Raycast for Walkable Layer
            int layerMask = 0;
            layerMask = layerMask | (1 << WALKABLE_LAYER);
            RaycastHit hitInfo;
            bool isWalkable = Physics.Raycast(ray, out hitInfo, maxRaycastDepth, layerMask);

            if (isWalkable)
            {
                Cursor.SetCursor(walkCursor, cursorHotspot, CursorMode.Auto);
                onMouseOverWalkable(hitInfo.point);   // Broadcast Mouse over Walkable
            }
            return isWalkable;            
        }          
    }
}
