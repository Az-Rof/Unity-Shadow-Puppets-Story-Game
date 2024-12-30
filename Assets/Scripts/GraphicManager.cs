using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettings : MonoBehaviour
{
    // Enum untuk kualitas grafik
    public enum GraphicsQuality
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    // Variabel untuk menyimpan kualitas grafik saat ini
    [SerializeField] private GraphicsQuality currentGraphicsQuality;

    // Button untuk mengatur kualitas grafik
    public Button lowButton, mediumButton, highButton;

    private void Start()
    {
        // Load kualitas grafik dari PlayerPrefs, jika ada
        if (PlayerPrefs.HasKey("GraphicsQuality"))
        {
            currentGraphicsQuality = (GraphicsQuality)PlayerPrefs.GetInt("GraphicsQuality");
        }
        else
        {
            currentGraphicsQuality = GraphicsQuality.Medium; // Default kualitas
        }

        // Terapkan kualitas grafik saat ini
        SetGraphicsQuality(currentGraphicsQuality);

        // Update tombol sesuai kualitas grafik
        UpdateButton();
    }

    // Fungsi untuk mengatur kualitas grafik
    public void SetGraphicsQuality(GraphicsQuality quality)
    {
        currentGraphicsQuality = quality;

        // Simpan pilihan kualitas ke PlayerPrefs
        PlayerPrefs.SetInt("GraphicsQuality", (int)quality);
        PlayerPrefs.Save();

        // Atur kualitas grafik berdasarkan nilai enum
        switch (quality)
        {
            case GraphicsQuality.Low:
                QualitySettings.SetQualityLevel(0, true); // Preset kualitas rendah
                break;
            case GraphicsQuality.Medium:
                QualitySettings.SetQualityLevel(1, true); // Preset kualitas sedang
                break;
            case GraphicsQuality.High:
                QualitySettings.SetQualityLevel(2, true); // Preset kualitas tinggi
                break;
        }
    }

    // Fungsi untuk mengatur tombol saat kualitas grafik berubah
    public void UpdateButton()
    {
        // Nonaktifkan semua tombol terlebih dahulu
        lowButton.interactable = currentGraphicsQuality != GraphicsQuality.Low;
        mediumButton.interactable = currentGraphicsQuality != GraphicsQuality.Medium;
        highButton.interactable = currentGraphicsQuality != GraphicsQuality.High;
    }

    // Fungsi untuk mengatur kualitas grafik saat button diklik
    public void OnLowButtonClicked()
    {
        SetGraphicsQuality(GraphicsQuality.Low);
        UpdateButton();
    }

    public void OnMediumButtonClicked()
    {
        SetGraphicsQuality(GraphicsQuality.Medium);
        UpdateButton();
    }

    public void OnHighButtonClicked()
    {
        SetGraphicsQuality(GraphicsQuality.High);
        UpdateButton();
    }
}
