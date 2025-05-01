using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour
{
    public static SoundEffects Instance { get; private set; }

    [System.Serializable]
    public class NamedAudioSource
    {
        public string name;
        public AudioSource source;
    }

    [Header("Named AudioSources (e.g. Bomb, Explosion, PickUp)")]
    public List<NamedAudioSource> namedSources = new List<NamedAudioSource>();

    private Dictionary<string, AudioSource> audioSources;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize dictionary
            audioSources = new Dictionary<string, AudioSource>();
            foreach (var item in namedSources)
            {
                if (!audioSources.ContainsKey(item.name))
                {
                    audioSources.Add(item.name, item.source);
                }
                else
                {
                    Debug.LogWarning($"Duplicate AudioSource name: {item.name}");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(string name)
    {
        if (audioSources.TryGetValue(name, out var source))
        {
            source.Play();
        }
        else
        {
            Debug.LogWarning($"Sound '{name}' not found.");
        }
    }

    public void StopSound(string name)
    {
        if (audioSources.TryGetValue(name, out var source))
        {
            source.Stop();
        }
        else
        {
            Debug.LogWarning($"Sound '{name}' not found.");
        }
    }
}
