using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Video;


public class MenuManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public AudioSource musicSource;
    public GameObject canva;

    public GameObject playButton;
    public GameObject settingsButton;
    public GameObject quitButton;
    public GameObject titleButton;

    public GameObject modeChoosingPanel;
    public GameObject inputConfigPanel;

    public Texture2D customCursor;
    public Vector2 hotspot = Vector2.zero;
    public CursorMode cursorMode = CursorMode.Auto;

    void Start()
    {
        Cursor.SetCursor(customCursor, hotspot, cursorMode);
    }

    public void OnPlayClicked()
    {
        HideMainMenu();
        modeChoosingPanel.SetActive(true);
    }

    public void OnStartClicked()
    {
        DontDestroyOnLoad(musicSource.gameObject);
        musicSource.Play();
        videoPlayer.Prepare();
        videoPlayer.loopPointReached += OnVideoEnd;
        StartCoroutine(PlayWhenReady());
    }
    private IEnumerator PlayWhenReady()
    {
        while (!videoPlayer.isPrepared)
            yield return null;

        canva.SetActive(false);
        videoPlayer.Play();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        vp.loopPointReached -= OnVideoEnd;
        SceneManager.LoadScene("blindBomb");
    }

    public void OnSettingsClicked()
    {
        HideMainMenu();
        inputConfigPanel.SetActive(true);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // Only shows in editor
    }

    private void HideMainMenu()
    {
        playButton.SetActive(false);
        settingsButton.SetActive(false);
        quitButton.SetActive(false);
    }

    // Optional: Back buttons in ModeChoosing/InputConfig can call this
    public void ShowMainMenu()
    {
        modeChoosingPanel.SetActive(false);
        inputConfigPanel.SetActive(false);

        playButton.SetActive(true);
        settingsButton.SetActive(true);
        quitButton.SetActive(true);
        titleButton.SetActive(true);
    }
}
