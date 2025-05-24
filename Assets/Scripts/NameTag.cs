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

    private GameObject nameTagInstance;
    private TextMeshProUGUI nameText;
    private RectTransform rectTransform;

    void Start()
    {
        InitiateNameTag();
    }
    void LateUpdate()
    {
        // This is called when character/game object change the localscale (rotating)
        fixedRotation();
    }

    void InitiateNameTag()
    {

        // Get character name from CharacterStats if available
        CharacterStats characterStats = GetComponent<CharacterStats>();

        // Create a canvas for nametag inside hierarchy of this gameobject
        GameObject canvasObject = new GameObject("NameTagCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;  // Set render mode to WorldSpace
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
        nameText.text = characterStats.name;
        nameText.fontSize = 1; // Set font size
        if (tag=="Enemy")
        {
            nameText.color = new Color32(255, 93, 93, 255); // Set text color    
        }
        else
        {
            nameText.color = Color.white; // Set text color
        }
        
        

        // Set location of the name tag to above the character
        rectTransform = nameTagInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localPosition = new Vector3(this.transform.position.x, this.transform.position.y + heightOffset, this.transform.position.z); // Set position above character
            rectTransform.localScale = new Vector3(1, 1, 1); // Set scale to 1
        }
        else
        {
            Debug.LogError("NameTag instance does not have a RectTransform component!");
        }
    }

    void fixedRotation()
    {
        if (transform.localScale.x < 0)
        {
           rectTransform.localScale = new Vector3(-1, 1, 1); // Set scale to 1
           
        }
        else
        {
            rectTransform.localScale = new Vector3(1, 1, 1); // Set scale to 1
        }
    }
}