using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LightFlickerScript : MonoBehaviour
{
    private bool isFlickering = false;
    public float timeDelay;

    void Update()
    {
        if(isFlickering == false) //don't start if the game is paused/over
        {
            StartCoroutine(FlickeringLight());
        }
    }

    IEnumerator FlickeringLight()
    {
        isFlickering = true;

        this.gameObject.GetComponent<Light>().enabled = false;

        timeDelay = Random.Range(0.25f, 1f); //this is the min and max of the flicker

        yield return new WaitForSeconds(timeDelay); //wait for the time delay

        this.gameObject.GetComponent<Light>().enabled = true;

        timeDelay = Random.Range(0.5f, 1.3f); 

        yield return new WaitForSeconds(timeDelay); 

        isFlickering = false;
    }
}