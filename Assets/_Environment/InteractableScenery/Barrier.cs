using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Characters;
using UnityEngine.UI;

public class Barrier : MonoBehaviour {

    [SerializeField] Text textBox;
    [SerializeField] GameObject standPos;
    [SerializeField] GameObject particlePrefab;

    bool isBeingDestroyed = false;

    public void DestroySelf()
    {
        GameObject particleSystem = Instantiate(particlePrefab, transform.position, transform.rotation);
        Destroy(particleSystem, particleSystem.GetComponent<ParticleSystem>().main.duration);
        textBox.text = " ";
        Destroy(gameObject, 0.3f);
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerControl playerControl = other.GetComponent<PlayerControl>();
        if (playerControl)
        {
            if (!isBeingDestroyed)
            {
                textBox.text = "Press Space to destroy barrier";
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                isBeingDestroyed = true;
                playerControl.MoveAndKick(standPos.transform.position, this.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerControl playerControl = other.GetComponent<PlayerControl>();
        if (playerControl)
        {
            textBox.text = " ";
        }
    }
}
