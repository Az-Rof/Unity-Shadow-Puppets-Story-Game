using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    // **Variables**
    public int maxHealth = 100; // Maximum health of the entity
    public int currentHealth; // Current health of the entity

    // **Initialization**
    void Start()
    {
        currentHealth = maxHealth; // Set current health to max health at the start
    }

    // **Take Damage Method**
    public void TakeDamage(int damage)
    {
        currentHealth -= damage; // Reduce health by the damage amount
        Debug.Log(gameObject.name + " took " + damage + " damage. Current health: " + currentHealth);

        // Check if health drops to 0 or below
        if (currentHealth <= 0)
        {
            Die(); // Call the Die method
        }
    }

    // **Heal Method**
    public void Heal(int amount)
    {
        currentHealth += amount; // Increase health by the heal amount
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Ensure health does not exceed max health
        Debug.Log(gameObject.name + " healed for " + amount + ". Current health: " + currentHealth);
    }

    // **Die Method**
    void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        // Add death logic here (e.g., play death animation, destroy object, etc.)
        Destroy(gameObject); // Destroy the game object (optional)
    }

    // **Optional: Reset Health**
    public void ResetHealth()
    {
        currentHealth = maxHealth; // Reset health to max
        Debug.Log(gameObject.name + " health reset to " + currentHealth);
    }
}