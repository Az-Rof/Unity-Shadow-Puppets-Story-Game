using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using System;

public class IndrajitNagapasa : MonoBehaviour
{
    [SerializeField] PlayableDirector playableDirector;
    // This script is used to call/invoke a cutscene (Timeline) in Unity.
    public void CallCutScene()
    {
        // Logic to call the cutscene
        playableDirector.Play();
    }


    // Player interacts with the cutscene
    // This method can be called when the player interacts with an object or trigger
    void OnTriggerStay2D(Collider2D collision)
    {
        // Check if the in the collider theres is a game object named "Indrajit"
        if (collision.CompareTag("Player") && GameObject.Find("Indrajit") != null  )
        {

            // Call the cutscene when Indrajit is lose, // and the player enters the trigger
            if (GameObject.Find("Indrajit").GetComponent<CharacterStats>().currentHealth <= 0)
            {
                CallCutScene();
            }
        }   
    }
}