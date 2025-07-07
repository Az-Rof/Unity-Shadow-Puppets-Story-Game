using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Singleton untuk mengelola instance AudioManager
    public Audio[] musicSounds, sfxSounds; // Array untuk menyimpan data suara musik dan SFX
    public AudioSource musicSource, sfxSource; // AudioSource untuk memutar musik dan SFX
    private string currentSceneName; // Nama scene aktif saat ini
    private bool isInitialized = false; // Flag untuk mencegah restart musik

    public Slider musicSlider;
    public Slider sfxSlider;

    public void Awake()
    {
        musicSlider = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "musicVolume")?.GetComponent<Slider>();
        sfxSlider = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "sfxVolume")?.GetComponent<Slider>();
        
        // Implementasi Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Pastikan AudioManager tidak dihancurkan saat berganti scene
            SceneManager.sceneLoaded += OnSceneLoaded; // Mendaftarkan event untuk menangani pergantian scene
            
            // Hanya mainkan musik saat pertama kali dibuat
            currentSceneName = SceneManager.GetActiveScene().name;
            PlayMusicForScene(currentSceneName);
            isInitialized = true;
        }
        else
        {
            Destroy(gameObject); // Hancurkan duplikat instance
            return;
        }
    }

    public void Start()
    {
        // Referensi ulang slider setiap kali scene dimuat
        musicSlider = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "musicVolume")?.GetComponent<Slider>();
        sfxSlider = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "sfxVolume")?.GetComponent<Slider>();

        // Atur nilai slider dari PlayerPrefs
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("musicVolume", musicSlider.value);
        if (sfxSlider != null)
            sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume", sfxSlider.value);

        // Ambil pengaturan volume musik dan SFX jika sudah disimpan
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            float musicVolume = PlayerPrefs.GetFloat("musicVolume");
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
            if (musicSlider != null)
            {
                musicSlider.value = musicVolume;
            }
        }
        if (PlayerPrefs.HasKey("sfxVolume"))
        {
            float sfxVolume = PlayerPrefs.GetFloat("sfxVolume");
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
            }
            if (sfxSlider != null)
            {
                sfxSlider.value = sfxVolume;
            }
        }
    }

    private void OnDestroy()
    {
        // Bersihkan event subscription saat AudioManager dihancurkan
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update nama scene saat ini
        currentSceneName = scene.name;
        
        // Hanya check musik jika sudah diinisialisasi sebelumnya
        if (isInitialized)
        {
            CheckMusicForScene(currentSceneName);
        }
    }

    private void CheckMusicForScene(string sceneName)
    {
        // Pastikan nama scene tidak null atau kosong
        if (string.IsNullOrEmpty(sceneName)) return;

        string requiredMusicName = null;

        // Tentukan musik yang diperlukan untuk scene ini
        switch (sceneName)
        {
            case "MainMenu":
                requiredMusicName = "PlayTheme";
                break;
            default:
                requiredMusicName = "PlayTheme"; // Default music untuk semua scene
                break;
        }

        // Cek apakah musik yang diperlukan sudah diputar
        if (!string.IsNullOrEmpty(requiredMusicName))
        {
            Audio targetSound = Array.Find(musicSounds, x => x.name == requiredMusicName);
            
            // Hanya ganti musik jika berbeda atau tidak sedang diputar
            if (targetSound != null && 
                (musicSource.clip != targetSound.audioClip || !musicSource.isPlaying))
            {
                PlayMusic(requiredMusicName);
            }
            else if (targetSound != null)
            {
                Debug.Log($"Music {requiredMusicName} is already playing for scene {sceneName}.");
            }
        }
    }

    private void PlayMusicForScene(string sceneName)
    {
        // Method ini hanya dipanggil saat pertama kali AudioManager dibuat
        if (string.IsNullOrEmpty(sceneName)) return;

        // Ganti musik berdasarkan nama scene
        switch (sceneName)
        {
            case "MainMenu":
                PlayMusic("PlayTheme");
                break;
            default:
                PlayMusic("PlayTheme");
                break;
        }
    }

    public void PlayMusic(string name)
    {
        // Pastikan AudioSource untuk musik tersedia
        if (musicSource == null)
        {
            Debug.LogWarning("MusicSource is missing or destroyed. Attempting to recreate.");
            musicSource = gameObject.AddComponent<AudioSource>(); // Tambahkan AudioSource jika hilang
        }

        // Temukan audio berdasarkan nama
        Audio sound = Array.Find(musicSounds, x => x.name == name);

        if (sound == null)
        {
            Debug.LogError($"Music sound not found: {name}"); // Jika audio tidak ditemukan
            return;
        }

        // Jangan mainkan lagi jika audio sudah diputar
        if (musicSource.clip == sound.audioClip && musicSource.isPlaying)
        {
            Debug.Log($"Music {name} is already playing.");
            return;
        }

        // Set the clip of the music source to the audio clip of the sound
        musicSource.clip = sound.audioClip;
        musicSource.loop = true; // Pastikan musik berulang
        // Play the music
        musicSource.Play();
        
        Debug.Log($"Now playing: {name}");
    }

    public void PlaySFX(string name)
    {
        // Temukan SFX berdasarkan nama
        Audio sound = Array.Find(sfxSounds, x => x.name == name);

        if (sound == null)
        {
            Debug.LogError($"SFX sound not found: {name}");
            return;
        }

        // Buat GameObject sementara untuk memutar SFX
        GameObject tempSFX = new GameObject($"SFX_{name}");
        AudioSource tempAudioSource = tempSFX.AddComponent<AudioSource>();

        // Set properti AudioSource sementara
        tempAudioSource.clip = sound.audioClip;
        tempAudioSource.volume = sfxSource != null ? sfxSource.volume : 1.0f; // Gunakan volume default jika null
        tempAudioSource.Play();

        // Hancurkan GameObject setelah selesai memutar suara
        Destroy(tempSFX, sound.audioClip.length);
    }

    public void SetMusicVolume(float volume)
    {
        // Pastikan AudioSource musik tersedia sebelum mengatur volume
        if (musicSource == null)
        {
            Debug.LogWarning("MusicSource is missing or destroyed. Cannot set volume.");
            return;
        }
        musicSource.volume = volume; // Atur volume musik
        PlayerPrefs.SetFloat("musicVolume", volume); // Simpan ke PlayerPrefs
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        // Pastikan AudioSource SFX tersedia sebelum mengatur volume
        if (sfxSource == null)
        {
            Debug.LogWarning("SFXSource is missing or destroyed. Cannot set volume.");
            return;
        }
        sfxSource.volume = volume; // Atur volume SFX
        PlayerPrefs.SetFloat("sfxVolume", volume); // Simpan ke PlayerPrefs
        PlayerPrefs.Save();
    }

    public void musicVolume()
    {
        if (musicSlider != null)
        {
            SetMusicVolume(musicSlider.value);
            PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
        }
    }

    public void sfxVolume()
    {
        if (sfxSlider != null)
        {
            SetSFXVolume(sfxSlider.value);
            PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
        }
    }

    void Update()
    {
        // Ambil pengaturan volume musik dan SFX jika sudah disimpan
        if (PlayerPrefs.HasKey("musicVolume") && musicSource != null)
        {
            float musicVolume = PlayerPrefs.GetFloat("musicVolume");
            musicSource.volume = musicVolume;
            if (musicSlider != null)
            {
                musicSlider.value = musicVolume;
            }
        }
        if (PlayerPrefs.HasKey("sfxVolume") && sfxSource != null)
        {
            float sfxVolume = PlayerPrefs.GetFloat("sfxVolume");
            sfxSource.volume = sfxVolume;
            if (sfxSlider != null)
            {
                sfxSlider.value = sfxVolume;
            }
        }
    }
}