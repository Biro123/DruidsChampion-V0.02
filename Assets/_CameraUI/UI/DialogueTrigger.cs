using UnityEngine;
using RPG.Characters;
using UnityEngine.UI;
using System;
using System.Collections;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] Text textBox;
    [SerializeField] string[] textLinesToDisplay;
    [SerializeField] float displayTime = 3f;
    [SerializeField] float triggerRadius = 5f;
    [SerializeField] bool isOneTimeOnly = true;

    bool hasPlayed = false;
    bool isPlaying = false;
    GameObject player;

    void Start()
    {
        player = FindObjectOfType<PlayerControl>().gameObject;
    }

    private void Update()
    {
        var distanceToPlayer = Vector3.Magnitude(transform.position - player.transform.position);
        if(distanceToPlayer <= triggerRadius)
        {
            RequestDisplayText();
        }
    }

    void RequestDisplayText()
    {
        if (isOneTimeOnly && hasPlayed)
        {
            return;
        }
        else if (!isPlaying)   
        {
            StartCoroutine(DisplayTextOverTime());
            hasPlayed = true;
        }
    }

    private IEnumerator DisplayTextOverTime()
    {
        isPlaying = true;

        foreach (string textLineToDisplay in textLinesToDisplay)
        {
            textBox.text = textLineToDisplay;
            yield return new WaitForSeconds(displayTime);
            textBox.text = null;
        }
        isPlaying = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0f, 255f, .5f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
