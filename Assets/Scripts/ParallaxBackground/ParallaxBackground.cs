using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Transform kamera utama. Akan otomatis terdeteksi jika kosong.")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Transform dari objek ini yang akan bergerak. Akan otomatis terdeteksi jika kosong.")]
    [SerializeField] private Transform subjectTransform;

    [Header("Parallax Effect")]
    [Tooltip("Seberapa banyak latar belakang bergerak relatif terhadap kamera. (0 = tidak bergerak, 1 = bergerak dengan kamera)")]
    [SerializeField] private Vector2 parallaxEffectMultiplier = new Vector2(0.5f, 0.5f);
    
    [Header("Infinite Scrolling Options")]
    [Tooltip("Aktifkan jika lapisan ini harus berulang secara horizontal.")]
    [SerializeField] private bool infiniteHorizontal = false;
    [Tooltip("Aktifkan jika lapisan ini harus berulang secara vertikal.")]
    [SerializeField] private bool infiniteVertical = false;

    private Vector3 subjectStartPosition; // Posisi awal objek parallax ini
    private Vector3 cameraStartPosition;  // Posisi awal kamera saat skrip dimulai
    private float textureUnitSizeX;       // Lebar sprite untuk scrolling horizontal tak terbatas
    private float textureUnitSizeY;       // Tinggi sprite untuk scrolling vertikal tak terbatas

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            if (cameraTransform == null)
            {
                Debug.LogError($"ParallaxBackground on {gameObject.name}: Main Camera not found. Please assign Camera Transform manually.", this);
                enabled = false; // Nonaktifkan skrip jika kamera tidak ditemukan
                return;
            }
        }

        if (subjectTransform == null)
        {
            subjectTransform = transform;
        }

        subjectStartPosition = subjectTransform.position;
        cameraStartPosition = cameraTransform.position;

        if (infiniteHorizontal || infiniteVertical)
        {
            SpriteRenderer spriteRenderer = subjectTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // Menggunakan bounds dari sprite untuk ukuran yang akurat
                textureUnitSizeX = spriteRenderer.bounds.size.x;
                textureUnitSizeY = spriteRenderer.bounds.size.y;

                if (textureUnitSizeX == 0 && infiniteHorizontal)
                {
                    Debug.LogWarning($"ParallaxBackground on {gameObject.name}: textureUnitSizeX is 0. Infinite horizontal scrolling might not work. Pastikan SpriteRenderer memiliki sprite yang valid dan ukuran yang benar.", this);
                    infiniteHorizontal = false; // Nonaktifkan jika ukuran tidak valid
                }
                if (textureUnitSizeY == 0 && infiniteVertical)
                {
                    Debug.LogWarning($"ParallaxBackground on {gameObject.name}: textureUnitSizeY is 0. Infinite vertical scrolling might not work. Pastikan SpriteRenderer memiliki sprite yang valid dan ukuran yang benar.", this);
                    infiniteVertical = false; // Nonaktifkan jika ukuran tidak valid
                }
            }
            else
            {
                Debug.LogWarning($"ParallaxBackground on {gameObject.name}: SpriteRenderer atau Sprite tidak ditemukan. Infinite scrolling dinonaktifkan.", this);
                infiniteHorizontal = false;
                infiniteVertical = false;
            }
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null || subjectTransform == null) return;

        // Hitung seberapa jauh kamera telah bergerak dari posisi awalnya
        Vector3 deltaCameraMovement = cameraTransform.position - cameraStartPosition;

        // Hitung posisi baru untuk subjek berdasarkan efek parallax
        float newX = subjectStartPosition.x + deltaCameraMovement.x * parallaxEffectMultiplier.x;
        float newY = subjectStartPosition.y + deltaCameraMovement.y * parallaxEffectMultiplier.y;

        subjectTransform.position = new Vector3(newX, newY, subjectTransform.position.z);

        // Logika untuk Infinite Scrolling
        // Bagian ini memeriksa apakah kamera telah bergerak cukup jauh sehingga latar belakang perlu "membungkus" (wrap around)
        // Variabel 'tempDist' menghitung seberapa banyak latar belakang telah "bergeser" relatif terhadap posisi awal kamera.
        // Ketika pergeseran ini melebihi lebar/tinggi satu unit tekstur, kita menyesuaikan 'subjectStartPosition'
        // untuk secara efektif memindahkan titik asal latar belakang.

        if (infiniteHorizontal && textureUnitSizeX > 0)
        {
            // Jarak efektif yang telah ditempuh kamera, dari perspektif latar belakang (mempertimbangkan (1 - parallaxEffect)).
            // Ini adalah seberapa jauh "jahitan" akan bergerak relatif terhadap posisi awal kamera.
            float tempDistX = cameraTransform.position.x * (1f - parallaxEffectMultiplier.x);

            if (tempDistX > subjectStartPosition.x + textureUnitSizeX)
            {
                subjectStartPosition.x += textureUnitSizeX;
            }
            else if (tempDistX < subjectStartPosition.x - textureUnitSizeX)
            {
                subjectStartPosition.x -= textureUnitSizeX;
            }
        }

        if (infiniteVertical && textureUnitSizeY > 0)
        {
            float tempDistY = cameraTransform.position.y * (1f - parallaxEffectMultiplier.y);

            if (tempDistY > subjectStartPosition.y + textureUnitSizeY)
            {
                subjectStartPosition.y += textureUnitSizeY;
            }
            else if (tempDistY < subjectStartPosition.y - textureUnitSizeY)
            {
                subjectStartPosition.y -= textureUnitSizeY;
            }
        }
    }
}
