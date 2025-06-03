using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;


public class NameTag : MonoBehaviour
{
    [Header("Name Tag Settings")]
    // public string characterName;
    public GameObject nameTagPrefab, healthSliderPrefab; // Prefab for the name tag
    [SerializeField] float heightOffset = 6.5f; // Offset in world space (relative to character height)
    [SerializeField] float healthSliderYOffset = 1.5f; // Offset for health slider position
    [SerializeField] Vector2 healthSliderSize = new Vector2(10, 1); // Size of the health slider

    GameObject nameTagInstance;
    TextMeshProUGUI nameText;
    Slider healthSlider;
    RectTransform nameTagrectTransform, healthSliderRectTransform;

    private CharacterStats stats;
    public CharacterStats _stats
    {
        get { return stats; }
        set { stats = value; }
    }


    void Start()
    {
        // Get character stats if available
        stats = GetComponent<CharacterStats>();

        // Create name tag
        InitiateNameTag();
        // Create health slider
        InitiateHealthSlider();

    }
    void LateUpdate()
    {
        // This is called when character/game object change the localscale (rotating)
        fixedRotation();
        // Sinkronisasi slider dengan currentHealth
        if (healthSlider != null && stats != null)
        {
            healthSlider.value = stats.currentHealth;
        }
    }

    void InitiateNameTag()
    {
        // Create a canvas for nametag inside hierarchy of this gameobject
        GameObject canvasObject = new GameObject("NameTagCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;  // Set render mode to WorldSpace
        canvas.sortingLayerName = "NameTag"; // Set sorting layer name
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        canvasObject.layer = LayerMask.NameToLayer("UI");

        // Set parent to this gameobject but in the first child position
        canvasObject.transform.SetParent(transform);
        canvasObject.transform.SetSiblingIndex(0);

        // Create name tag instance
        nameTagInstance = Instantiate(nameTagPrefab, canvasObject.transform);
        nameTagInstance.transform.localPosition = Vector3.zero; // Reset local position

        // Get TextMeshProUGUI component
        nameText = nameTagInstance.GetComponent<TextMeshProUGUI>();

        // Set character name
        nameText.text = stats.name;
        nameText.fontSize = 1; // Set font size
        if (tag == "Enemy")
        {
            nameText.color = new Color32(255, 93, 93, 255); // Set text color    
        }
        else
        {
            nameText.color = Color.white; // Set text color
        }

        // Set location of the name tag to above the character
        nameTagrectTransform = nameTagInstance.GetComponent<RectTransform>();
        if (nameTagrectTransform != null)
        {
            nameTagrectTransform.localPosition = new Vector3(this.transform.position.x, this.transform.position.y + heightOffset, this.transform.position.z); // Set position above character
            nameTagrectTransform.localScale = new Vector3(1, 1, 1); // Set scale to 1
        }
        else
        {
            Debug.LogError("NameTag instance does not have a RectTransform component!");
        }
    }

    void InitiateHealthSlider()
    {
        // Create a canvas for health slider inside hierarchy of this gameobject
        GameObject canvasObject = gameObject.transform.Find("NameTagCanvas").gameObject; 
        if (this.tag != "Player" && healthSliderPrefab != null)
        {
            healthSlider = Instantiate(healthSliderPrefab, canvasObject.transform).GetComponent<Slider>();
            healthSliderRectTransform = healthSlider.GetComponent<RectTransform>();
            if (healthSliderRectTransform != null)
            {
                healthSliderRectTransform.localPosition = new Vector3(this.transform.position.x, this.transform.position.y + heightOffset - healthSliderYOffset, this.transform.position.z); // Set position above character
                healthSliderRectTransform.localScale = new Vector3(1, 1, 1); // Set scale to 1
                healthSlider.maxValue = stats.maxHealth; // Set initial health value
                healthSliderRectTransform.sizeDelta = healthSliderSize; // Set size of the health slider

            }
            else
            {
                Debug.LogError("HealthSlider instance does not have a RectTransform component!");
            }

        }
        else
        {
            Debug.LogWarning("Health slider already exists, skipping instantiation.");
        }

    }
    void fixedRotation()
    {

        if (transform.localScale.x < 0)
        {
            nameTagrectTransform.localScale = new Vector3(-1, 1, 1); // Set scale to 1
            if (healthSliderPrefab != null)
            {
                healthSliderRectTransform.localScale = new Vector3(-1, 1, 1); // Set scale to 1
            }
        }
        else
        {
            nameTagrectTransform.localScale = new Vector3(1, 1, 1); // Set scale to 1
            if (healthSliderPrefab != null)
            {
                healthSliderRectTransform.localScale = new Vector3(1, 1, 1); // Set scale to 1
            }
        }
    }
}