using UnityEngine;
using UnityEngine.UI;
using System.Collections;



public class CharacterStats : MonoBehaviour {
    [Header("Character Info")]
    public string CharacterName;
    public string CharacterType; 

    // Basic Stats
    [Header("Basic Stats")]
    public float maxHealth;
    public float maxStamina;
    public float attackPower;
    public float defensePower;

    // Current Stats
    [Header("CurrentStats")]
    public float currentHealth;
    public float currentStamina;
    
    // Sliders Stats 
    [SerializeField] public Slider healthSlider, staminaSlider; 
    
    // Slider yang akan digunakan
    [SerializeField] public Text charnameText;

    void Start(){
        // Inisialisasi nama & peran character
        CharacterName = this.gameObject.name ;
        charnameText.text = CharacterName;
        CharacterType = this.gameObject.tag;

        // Inisialisasi Basic Stats 
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        // Inisialiasi sliders
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = currentStamina;
    }
}