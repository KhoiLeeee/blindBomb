using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }
    public AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void PlayMusic()
    {
        if (!audioSource.isPlaying)
        {

            audioSource.volume = VolumeManager.GetVolume("music");
            audioSource.Play();
        }
    }
}
