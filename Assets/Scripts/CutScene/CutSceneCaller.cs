using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using System;

public class CutSceneCaller : MonoBehaviour
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
        if (collision.CompareTag("Player"))
        {
            // Call the cutscene when the player enters the trigger
            CallCutScene();
        }   
    }
}