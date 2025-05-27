using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CharacterStats : MonoBehaviour
{
    [Header("Character Info")]
    public string CharacterName;
    public string CharacterType;

    // Basic Stats
    [Header("Basic Stats")]
    public float maxHealth;
    public float maxStamina;
    public float attackPower;
    public float attackRange;
    public float attackCooldown;
    public float defensePower;
    public float speed;
    public float jumpPower;
    public float jumpCooldown;
    public float dashPower;
    public float dashCooldown;


    [SerializeField] public float healthRegenRate, staminaRegenRate;

    // Current Stats
    [Header("CurrentStats")]
    public float currentHealth;
    public float currentStamina;

    [Header("Cost Of Actions")] // Cost of actions
    // [SerializeField] float dashCost, jumpCost, attackCost;
    private float lastAttackTime = 0f;
    public List<TakeActionCost> actionCosts = new List<TakeActionCost>() // List to hold action costs
    {
        new TakeActionCost() { ActionName = "Dash", CostValue = 10 },
        new TakeActionCost() { ActionName = "Jump", CostValue = 10 },
        new TakeActionCost() { ActionName = "Attack", CostValue = 10 },
    };

    // Set the Standar Action for action costs 


    void Start()
    {
        InitiateCharacterStats();
        InitiateSliders();

        // Update the current health and stamina value
        StartCoroutine(Regenerate());
    }

    void Update()
    {

    }

    void InitiateCharacterStats()
    {
        // Inisialisasi nama & peran character
        CharacterName = this.gameObject.name;
        CharacterType = this.gameObject.tag;

        // Inisialisasi Basic Stats 
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        // Inisialiasi sliders was moved to NameTag.cs

    }
    void InitiateSliders()
    {
        // Find the NameTagCanvas object
        GameObject canvasObject = GameObject.Find("NameTagCanvas");
    }

    public IEnumerator Regenerate()
    {
        while (true)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate / 100 * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
                //staminaSlider.value = currentStamina; // Update stamina slider
            }

            if (currentHealth < maxHealth)
            {
                currentHealth += healthRegenRate / 100 * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
                //healthSlider.value = currentHealth; // Update health slider
            }
            yield return null; // Wait for the next frame
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= (damage - defensePower); // Reduce health by the damage amount
        //healthSlider.value = currentHealth; // Update health slider
        if (currentHealth <= 0)
        {
            if (tag != "Player")
            {
                Die(); // Call the Die method if health drops to 0 or below    
            }
            else
            {
                Debug.Log(gameObject.name + " has died. Game Over.");
                Time.timeScale = 0; // Pause the game
            }

        }
    }


    // Method to check if the character can perform a dash action
    public bool CanAction()
    {
        foreach (TakeActionCost actionCost in actionCosts)
        {
            if (currentStamina <= actionCost.CostValue)
                return false; // If current stamina is not enough for any action, return false
        }
        return true; // If stamina is enough for all actions, return true
    }

    public void TakeAction(float actionValue)
    {
        // Check if the character has enough stamina to perform the action
        if (CanAction())
        {
            currentStamina -= actionValue; // Reduce stamina by the action value
            currentStamina = Mathf.Max(currentStamina, 0); // Ensure stamina doesn't go below 0   
        }
    }

    // Fungsi untuk mengambil cost berdasarkan nama aksi
    public float GetActionCost(string actionName)
    {
        foreach (var action in actionCosts)
        {
            if (action.ActionName == actionName)
                return action.CostValue;
        }
        return 0f; // Default jika tidak ditemukan
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        //Destroy this game object
        Destroy(this.gameObject); // Destroy the character game object
    }
}