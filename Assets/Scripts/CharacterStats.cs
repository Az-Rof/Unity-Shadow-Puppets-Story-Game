using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Playables;

public class CharacterStats : MonoBehaviour
{
    [Header("Character Info")]
    public string CharacterName;
    public string CharacterType;
    [SerializeField] bool isthisImportantCharacter = false; // Flag to check if this is an important character

    Enemy enemyScript; // Reference to the Enemy script if this character is an enemy

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

    [Header("Cutscene on Death")]
    [SerializeField] private PlayableDirector deathCutscene;
    private bool hasTriggeredDeath = false; // Flag untuk memastikan cutscene hanya play sekali


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

    // Set the Standard Action for action costs

    void Awake()
    {

    }

    void Start()
    {
        // Initialize character stats

        InitiateSliders();
        InitiateCharacterStats();
        InitiateEnemy();
        // Update the current health and stamina value
        StartCoroutine(Regenerate());
    }

    void Update()
    {
        // Kondisi kematian sekarang hanya akan memicu event satu kali saja
        if (currentHealth <= 0 && !hasTriggeredDeath)
        {
            hasTriggeredDeath = true; // Langsung set flag agar tidak ter-trigger lagi

            if (tag == "Enemy" && !isthisImportantCharacter)
            {
                Debug.Log(gameObject.name + " has died. Enemy defeated.");
                Die();
            }
            else if (tag == "Player" && isthisImportantCharacter)
            {
                Debug.Log(gameObject.name + " has died. Game Over.");
                // Logika Game Over
            }
            else if (tag == "Enemy" && isthisImportantCharacter)
            {
                Debug.Log(gameObject.name + " has died. Important Enemy defeated.");

                if (enemyScript != null)
                {
                    enemyScript.enabled = false; // Matikan skrip AI musuh
                }

                // PERUBAHAN UTAMA: Putar cutscene dari sini
                if (deathCutscene != null)
                {
                    deathCutscene.Play();
                }
                else
                {
                    Debug.LogWarning("Death cutscene belum di-set di Inspector untuk " + gameObject.name);
                }
            }
        }
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

    void InitiateEnemy()
    {
        if (tag == "Enemy")
        {
            enemyScript = GetComponent<Enemy>();
        }
        else
        {
            return; // If not an enemy, exit the method
        }
    }

    public IEnumerator Regenerate()
    {
        while (true)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate / 100 * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }

            if (currentHealth < maxHealth)
            {
                currentHealth += healthRegenRate / 100 * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
            yield return null; // Wait for the next frame
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= (damage - defensePower); // Reduce health by the damage amount
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