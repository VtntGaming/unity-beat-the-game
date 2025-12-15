using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Footstep (looping)")]
    [SerializeField] private AudioSource footstepSource;  // set Loop = true in inspector

    [Header("Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip[] sfxClips;        // for enum-driven effects

    private Dictionary<Sound, AudioClip> _clipMap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // build the enum-to-clip map
            _clipMap = new Dictionary<Sound, AudioClip>();
            var values = System.Enum.GetValues(typeof(Sound));
            for (int i = 0; i < values.Length; i++)
            {
                var key = (Sound)values.GetValue(i);
                if (i < sfxClips.Length)
                    _clipMap[key] = sfxClips[i];
                else
                    Debug.LogWarning($"Missing SFX clip for {key}");
            }

            // configure footstepSource
            if (footstepSource != null && footstepClip != null)
            {
                footstepSource.clip = footstepClip;
                footstepSource.loop = true;
                footstepSource.playOnAwake = false;
            }
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // one‐shot SFX
    public void PlaySound(Sound s)
    {
        if (sfxSource == null || !_clipMap.ContainsKey(s)) return;
        var clip = _clipMap[s];
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    // FOOTSTEP LOOP CONTROL:
    public void StartFootsteps()
    {
        if (footstepSource != null && !footstepSource.isPlaying)
            footstepSource.Play();
    }

    public void StopFootsteps()
    {
        if (footstepSource != null && footstepSource.isPlaying)
            footstepSource.Stop();
    }

    // static facades (optional)
    public static void Sfx(Sound s) => Instance?.PlaySound(s);
    public static void FootstepsStart() => Instance?.StartFootsteps();
    public static void FootstepsStop() => Instance?.StopFootsteps();
}
