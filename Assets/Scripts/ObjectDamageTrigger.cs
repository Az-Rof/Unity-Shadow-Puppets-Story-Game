using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ObjectDamageTrigger : MonoBehaviour
{
    // Enum to define the type of damage
    [Header("Falling Death/Damage Trigger Settings")]
    [SerializeField] private DamageType damageType;
    [SerializeField] private float manualDamage;

    // get playercontroller and the character stats
    private PlayerController playerController;
    private CharacterStats characterStats;

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {

            playerController = collision.GetComponent<PlayerController>();
            characterStats = collision.GetComponent<CharacterStats>();
            // Check if playerController and characterStats are not null
            if (playerController != null && characterStats != null)
            {
                float damage;

                switch (damageType)
                {
                    case DamageType.GameOver:
                        damage = characterStats.maxHealth - 1f; // Set health to 1 less than max health for Game Over
                        break;
                    case DamageType.Manual:
                        damage = manualDamage;
                        break;
                    default:
                        damage = 0;
                        break;
                }

                // Apply damage to the player
                characterStats.TakeDamage((int)damage);
            }
        }
    }

    // This method is called when the player stays in the trigger collider
    // This is useful for continuous damage, like in a lava pit or poison gas
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            characterStats = other.GetComponent<CharacterStats>();
            // Check if playerController and characterStats are not null
            if (playerController != null && characterStats != null)
            {
                float damage;

                switch (damageType)
                {
                    case DamageType.GameOver:
                        damage = characterStats.maxHealth - 1f; // Set health to 1 less than max health for Game Over
                        break;
                    case DamageType.Manual:
                        damage = manualDamage;
                        break;
                    default:
                        damage = 0;
                        break;
                }

                // Apply damage to the player
                characterStats.TakeDamage((int)damage);
            }
        }
    }
}

public enum DamageType
{
    GameOver,
    Manual
}

