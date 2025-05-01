using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        if (SoundEffects.Instance != null)
            SoundEffects.Instance.PlaySound("Click");
        else
            Debug.LogWarning("SoundEffects.Instance is null.");
    }
}
