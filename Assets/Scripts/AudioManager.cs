using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Singleton untuk mengelola instance AudioManager
    public Audio[] musicSounds, sfxSounds; // Array untuk menyimpan data suara musik dan SFX
    public AudioSource musicSource, sfxSource; // AudioSource untuk memutar musik dan SFX
    private string currentSceneName; // Nama scene aktif saat ini

    public void Awake()
    {
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
        // Ambil pengaturan volume musik dan SFX jika sudah disimpan
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            musicSource.volume = PlayerPrefs.GetFloat("musicVolume");
        }
        if (PlayerPrefs.HasKey("sfxVolume"))
        {
            sfxSource.volume = PlayerPrefs.GetFloat("sfxVolume");
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
            case "SampleScene":
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

        // Set klip musik dan mulai mainkan
        musicSource.clip = sound.audioClip;
        musicSource.Play();
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
    }
}