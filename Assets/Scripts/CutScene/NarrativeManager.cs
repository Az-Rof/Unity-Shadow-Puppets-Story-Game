using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TMPro;

/// <summary>
/// Mengatur naratif TextMeshProUGUI dalam game berdasarkan timing di Timeline.
/// Melakukan fade in dan fade out berdasarkan waktu aktivasi dari Activation Track.
/// </summary>
public class NarrativeManager : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector timelineDirector;

    // Menyimpan waktu start dan end masing-masing GameObject naratif
    private Dictionary<GameObject, (double start, double end)> narrativeTimings = new Dictionary<GameObject, (double, double)>();

    // Menyimpan referensi ke TextMeshProUGUI masing-masing naratif
    private Dictionary<GameObject, TextMeshProUGUI> narrativeTexts = new Dictionary<GameObject, TextMeshProUGUI>();

    private void Start()
    {
        if (timelineDirector == null)
        {
            Debug.LogError("PlayableDirector belum di-assign.");
            return;
        }

        // Temukan semua anak dengan komponen TMP dan nonaktifkan mereka
        var narratives = GetComponentsInChildren<TextMeshProUGUI>(true)
                         .Select(tmp => tmp.gameObject)
                         .ToList();

        foreach (var go in narratives)
        {
            go.SetActive(false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
                narrativeTexts[go] = tmp;
        }

        // Ambil waktu aktivasi dari Timeline
        ExtractClipTimings(narratives);

        // Mulai monitor fade berdasarkan waktu
        StartCoroutine(FadeNarrativesByTimeline());
    }

    /// <summary>
    /// Mengambil data timing dari Activation Track di Timeline
    /// </summary>
    private void ExtractClipTimings(List<GameObject> narrativeObjects)
    {
        if (!(timelineDirector.playableAsset is TimelineAsset timelineAsset))
            return;

        foreach (var track in timelineAsset.GetOutputTracks())
        {
            if (track is ActivationTrack)
            {
                var target = timelineDirector.GetGenericBinding(track) as GameObject;

                if (target != null && narrativeObjects.Contains(target))
                {
                    foreach (var clip in track.GetClips())
                        narrativeTimings[target] = (clip.start, clip.end);
                }
            }
        }
    }

    /// <summary>
    /// Mengecek waktu Timeline dan mengatur fade naratif sesuai timing-nya
    /// </summary>
    private IEnumerator FadeNarrativesByTimeline()
    {
        while (timelineDirector.time < timelineDirector.duration)
        {
            double currentTime = timelineDirector.time;

            foreach (var kvp in narrativeTimings)
            {
                var go = kvp.Key;
                var (start, end) = kvp.Value;

                // Fade In saat clip mulai
                if (currentTime >= start && currentTime < start + 0.1 && !go.activeSelf)
                {
                    go.SetActive(true);
                    var tmp = narrativeTexts[go];
                    if (tmp != null)
                        StartCoroutine(FadeTMPAlpha(tmp, 0f, 1f, 1f));
                }

                // Fade Out menjelang akhir clip
                if (currentTime >= end - 1f && currentTime < end && go.activeSelf)
                {
                    var tmp = narrativeTexts[go];
                    if (tmp != null)
                        StartCoroutine(FadeTMPAlpha(tmp, 1f, 0f, 1f, true));
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Melakukan animasi alpha pada komponen TMP, dengan opsi disable di akhir
    /// </summary>
    private IEnumerator FadeTMPAlpha(TextMeshProUGUI tmp, float from, float to, float duration, bool disableAfter = false)
    {
        float elapsed = 0f;
        Color color = tmp.color;
        color.a = from;
        tmp.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothAlpha = Mathf.SmoothStep(from, to, t); // Smooth fading
            color.a = smoothAlpha;
            tmp.color = color;
            yield return null;
        }

        color.a = to;
        tmp.color = color;

        if (disableAfter && to == 0f)
            tmp.gameObject.SetActive(false);
    }
}
