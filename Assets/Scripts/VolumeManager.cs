using UnityEngine;

public static class VolumeManager
{
    private static float musicVolume = 1f;
    private static float soundVolume = 1f;
    static VolumeManager()
    {
        musicVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        soundVolume = PlayerPrefs.GetFloat("soundVolume", 1f);
    }
    public static void SetVolume(float newVolume, string type)
    {
        newVolume = Mathf.Clamp01(newVolume);

        if (type == "music")
        {
            musicVolume = newVolume;
            PlayerPrefs.SetFloat("musicVolume", musicVolume);
            UpdateMusicSources();
        }
        else if (type == "sound")
        {
            soundVolume = newVolume;
            PlayerPrefs.SetFloat("musicVolume", musicVolume);
            UpdateSoundSources();
        }
    }

    public static float GetVolume(string type)
    {
        if (type == "music") return musicVolume;
        if (type == "sound") return soundVolume;
        return 1f;
    }

    private static void UpdateMusicSources()
    {
        foreach (var source in GameObject.FindGameObjectsWithTag("Music"))
        {
            AudioSource audio = source.GetComponent<AudioSource>();
            if (audio != null) audio.volume = musicVolume;
        }
    }

    private static void UpdateSoundSources()
    {
        foreach (var source in GameObject.FindGameObjectsWithTag("Sound"))
        {
            AudioSource audio = source.GetComponent<AudioSource>();
            if (audio != null) audio.volume = soundVolume;
        }
    }
}
