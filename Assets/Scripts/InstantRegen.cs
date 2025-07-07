using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantRegen : MonoBehaviour
{
    [Header("Instant Regen Settings")]
    [SerializeField] private RegenType regenType;
    [Tooltip("The flat amount of health or stamina to restore.")]
    [SerializeField] private float amount = 10f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterStats characterStats = other.GetComponent<CharacterStats>();
            if (characterStats != null)
            {
                AudioManager.Instance.PlaySFX("Regen");
                switch (regenType)
                {
                    case RegenType.Health:
                        if (characterStats.currentHealth < characterStats.maxHealth)
                        {
                            characterStats.currentHealth += amount;
                            characterStats.currentHealth = Mathf.Min(characterStats.currentHealth, characterStats.maxHealth);
                            Debug.Log("Player healed by " + amount + " health.");
                        }
                        break;
                    case RegenType.Stamina:
                        if (characterStats.currentStamina < characterStats.maxStamina)
                        {
                            characterStats.currentStamina += amount;
                            characterStats.currentStamina = Mathf.Min(characterStats.currentStamina, characterStats.maxStamina);
                            Debug.Log("Player stamina restored by " + amount + ".");
                        }
                        break;
                }
            }
            // Destroy the pickup object after it's used.
            Destroy(gameObject);
        }
    }
}
public enum RegenType
{
    Health,
    Stamina
}
