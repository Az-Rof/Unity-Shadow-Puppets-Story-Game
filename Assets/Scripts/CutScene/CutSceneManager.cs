using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneManager : MonoBehaviour
{
    [SerializeField] PlayerController player; // Reference to PlayerController script (if needed)
    
    [SerializeField] Enemy[] enemies; // Reference to Enemy scripts to handle multiple enemies

    [SerializeField] GameObject GUI; // Reference to the UI/UX elements that should be deactivated during cutscenes

    public void EnterCutsceneState()
    {
        Debug.Log("Entering Cutscene State...");

        // --- Player ---
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();

        }
        if (player != null)
        {
            // Disable player controls input and interactions
            player.inputActions.Player.Disable(); // Disable player input actions
            
        }
        else
        {
            // Player is essential, so we log an error and stop if not found.
            Debug.LogError("PlayerController not found in the scene. Please assign it in the inspector or ensure it exists in the scene.");
            return;
        }

        // --- Player GUI ---
        if (GUI == null)
        {
            GUI = GameObject.Find("GUI"); // Assuming the UI GameObject is named "UI"
        }

        if (GUI != null)
        {
            GUI.SetActive(false); // Deactivate UI elements
        }
        else
        {
            // UI is also likely essential to hide, so we log an error and stop.
            Debug.LogError("UI GameObject not found in the scene. Please assign it in the inspector or ensure it exists in the scene.");
            return;
        }

        // --- Enemies ---
        // Attempt to find all enemies if the array is not assigned or empty.
        if (enemies == null || enemies.Length == 0)
        {
            enemies = FindObjectsOfType<Enemy>();
        }

        // If any enemies are found (either assigned or found), disable them.
        if (enemies != null && enemies.Length > 0)
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.enabled = false; // Disable enemy controls
                }
            }
            Debug.Log($"Deactivated controls for {enemies.Length} enemies.");
        }
        else
        {
            // If no enemies are found, it might be intentional for some cutscenes.
            Debug.LogWarning("No enemies found in the scene or assigned. This might be intentional. No enemies will be disabled.");
        }
    }

    public void ExitCutsceneState()
    {
        Debug.Log("Exiting Cutscene State...");

        // --- Player ---
        if (player != null)
        {
            player.inputActions.Player.Enable(); // Enable player input actions
            Debug.Log("Player controls activated.");
        }
        else
        {
            Debug.LogWarning("PlayerController reference is not set in CutSceneManager. Cannot enable player controls.");
        }

        // --- Player GUI ---
        if (GUI != null)
        {
            GUI.SetActive(true); // Activate UI elements
            Debug.Log("UI elements activated.");
        }
        else
        {
            Debug.LogWarning("GUI reference is not set in CutSceneManager. Cannot activate UI.");
        }

        // --- Enemies ---
        if (enemies != null && enemies.Length > 0)
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.enabled = true; // Enable enemy controls
                }
            }
            Debug.Log($"Activated controls for {enemies.Length} enemies.");
        }
        else
        {
            Debug.LogWarning("Enemies reference is not set in CutSceneManager. Cannot enable enemy controls.");
        }
    }
}
