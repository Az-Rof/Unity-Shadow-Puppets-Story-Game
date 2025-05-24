using UnityEngine;
using UnityEngine.UI;
using System.Collections;



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

    void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject); // Destroy the game object
    }
}