using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiLightFlickering : MonoBehaviour
{
    private bool isFlickering = false;
    public float timeDelay;

    private Light[] allLights;

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
    }

    IEnumerator FlickeringLight()
    {
        isFlickering = true;

        foreach (Light light in allLights) {light.enabled = false;}

        timeDelay = Random.Range(0.25f, 1f); //this is the min and max of the flicker

        yield return new WaitForSeconds(timeDelay); //wait for the time delay

        foreach (Light light in allLights) { light.enabled = true; }

        timeDelay = Random.Range(0.5f, 1.3f);

        yield return new WaitForSeconds(timeDelay);

        isFlickering = false;
    }
}
