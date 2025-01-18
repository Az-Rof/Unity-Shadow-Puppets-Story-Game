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

    public Slider musicSlider;
    public Slider sfxSlider;
    // public Slider musicSlider, sfxSlider; // Sliders untuk volume musik dan SFX

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
        }
        else
        {
            Destroy(gameObject); // Hancurkan duplikat instance
            return;
        }

        // Ambil nama scene aktif dan mulai mainkan musik
        currentSceneName = SceneManager.GetActiveScene().name;
        PlayMusicForScene(currentSceneName);
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
            musicSource.volume = musicVolume;
            if (musicSlider != null)
            {
                musicSlider.value = musicVolume;
            }
        }
        if (PlayerPrefs.HasKey("sfxVolume"))
        {
            float sfxVolume = PlayerPrefs.GetFloat("sfxVolume");
            sfxSource.volume = sfxVolume;
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
        // Ganti musik sesuai dengan scene yang dimuat
        currentSceneName = scene.name;
        PlayMusicForScene(currentSceneName);
    }

    private void PlayMusicForScene(string sceneName)
    {
        // Pastikan nama scene tidak null atau kosong
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
        }
        else
        {
            // Set the clip of the music source to the audio clip of the sound
            musicSource.clip = sound.audioClip;
            // Play the music
            musicSource.Play();
        }
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
        SetMusicVolume(musicSlider.value);
        PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
    }

    public void sfxVolume()
    {
        SetSFXVolume(sfxSlider.value);
        PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
    }
    void Update()
    {
        // Ambil pengaturan volume musik dan SFX jika sudah disimpan
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            float musicVolume = PlayerPrefs.GetFloat("musicVolume");
            musicSource.volume = musicVolume;
            if (musicSlider != null)
            {
                musicSlider.value = musicVolume;
            }
        }
        if (PlayerPrefs.HasKey("sfxVolume"))
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