using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneManager : MonoBehaviour
{
    PlayerController player; // Reference to PlayerController script
    public PlayerController Player
    {
        get { return player; }
        set { player = value; }
    }

    Enemy enemy; // Reference to Enemy script (if needed)
    public Enemy Enemy
    {
        get { return enemy; }
        set { enemy = value; }
    }

    [SerializeField] GameObject GUI; // Reference to the UI/UX elements that should be deactivated during cutscenes

    public void EnterCutsceneState()
    {
        Debug.Log("Entering Cutscene State...");

        // Player
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
            if (player == null)
            {
                Debug.LogError("PlayerController not found in the scene. Please assign it in the inspector or ensure it exists in the scene.");
                return;
            }
            else
            {
                player.enabled = false; // Disable player controls
            }
        }
        // Player GUI
        if (GUI == null)
        {
            GUI = GameObject.Find("GUI"); // Assuming the UI GameObject is named "UI"
            if (GUI == null)
            {
                Debug.LogError("UI GameObject not found in the scene. Please assign it in the inspector or ensure it exists in the scene.");
                return;
            }
            else
            {
                GUI.SetActive(false); // Deactivate UI elements
            }
        }
        else
        {
            GUI.SetActive(false); // Deactivate UI elements
        }


        // Non activate other game elements if needed
        // Non activate enemy.cs
        if (enemy == null)
        {
            enemy = FindObjectOfType<Enemy>();
            Debug.LogError("Enemy not found in the scene. Please assign it in the inspector or ensure it exists in the scene.");
            return;
        }
        else if (enemy != null)
        {
            enemy.enabled = false; // Disable enemy controls if needed
            Debug.Log("Enemy controls deactivated.");
        }
        else
        {
            Debug.LogWarning("Enemy reference is not set in CutSceneManager.");
        }

    }

    public void ExitCutsceneState()
    {
        Debug.Log("Exiting Cutscene State...");

        // Activate player controls & UI/UX elements
        if (player != null && GUI != null)
        {
            player.enabled = true; // Enable player controls
            GUI.SetActive(true); // Activate UI elements
            player.gameObject.tag = "Player"; // Reset tag to allow interactions
            Debug.Log("Player controls and UI elements activated.");
        }
        else
        {
            Debug.LogWarning("PlayerController reference is not set in CutSceneManager.");
        }
        // Activate other game elements if needed
        // Activate enemy.cs
        if (enemy != null)
        {
            enemy.enabled = true; // Enable enemy controls if needed
            Debug.Log("Enemy controls activated.");
        }
        else
        {
            Debug.LogWarning("Enemy reference is not set in CutSceneManager.");
        }
    }
}
