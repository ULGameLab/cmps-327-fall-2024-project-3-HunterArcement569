using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiLightFlickering : MonoBehaviour
{
    private bool isFlickering = false;
    public float timeDelay;

    private Light[] allLights;
    private Light firstLight;
    private Light secondLight;

    void Update()
    {
        if (isFlickering == false) //don't start if the game is paused/over
        {
            StartCoroutine(FlickeringLight());
        }
    }

    private void Start()
    {
        allLights = GetComponentsInChildren<Light>();
        firstLight = allLights[0];
        secondLight = allLights[1];
    }

    IEnumerator FlickeringLight()
    {
        isFlickering = true;

        firstLight.enabled = false;
        secondLight.enabled = false;

        timeDelay = Random.Range(0.25f, 1f); //this is the min and max of the flicker

        yield return new WaitForSeconds(timeDelay); //wait for the time delay

        firstLight.enabled = true;
        secondLight.enabled = true;

        timeDelay = Random.Range(0.5f, 1.3f);

        yield return new WaitForSeconds(timeDelay);

        isFlickering = false;
    }
}
