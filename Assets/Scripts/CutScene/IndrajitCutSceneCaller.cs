using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using System;

public class IndrajitCutSceneCaller : MonoBehaviour
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
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the in the collider theres is an enemy
        if (collision.CompareTag("Player") && GameObject.Find("Buto") != null)
        {
            // Call the cutscene when the player enters the trigger
            CallCutScene();
        }   
    }
}