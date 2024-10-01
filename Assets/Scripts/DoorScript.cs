using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    
    Animator animator;
    private bool doorOpen = false;
    private bool playerInTrigger;

    void Start()
    {
        //get the animator attached to this game object
        animator = GetComponent<Animator>();
    }

    
    void Update()
    {
        //check that the player is holding the button and the player is in the trigger
        if (Input.GetKeyDown(KeyCode.F) && playerInTrigger)
        {
            doorOpen = !doorOpen; //invert the bool
            StartCoroutine(doorAnimation(doorOpen)); //start the coroutine
        }
    }

    //checks on if the player is entering, staying in, and exitting the trigger for the door animation
    private void OnTriggerEnter(Collider other)
    {
        playerInTrigger = true;
    }
    private void OnTriggerStay(Collider other)
    {
        playerInTrigger = true;
    }
    private void OnTriggerExit(Collider other)
    {
        playerInTrigger = false;
    }

    //coroutine for the animation
    private IEnumerator doorAnimation(bool value)
    {
        //changes depending on if the bool sent in is true (open the door) or false (close the door)
        if (value)
        {
            //set the trigger
            animator.SetTrigger("Open Door");
            //wait
            yield return new WaitForSeconds(0.667f);
            //reset the trigger
            animator.ResetTrigger("Open Door");
        }
        else
        {
            //set the trigger
            animator.SetTrigger("Close Door");
            //wait
            yield return new WaitForSeconds(0.667f);
            //reset the trigger
            animator.ResetTrigger("Close Door");
        }
    }
}
