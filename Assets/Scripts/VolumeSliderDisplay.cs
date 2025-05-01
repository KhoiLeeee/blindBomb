using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeSliderDisplay : MonoBehaviour
{
    public Slider slider;
    public TMP_Text valueText;
    public string volumeType; // "music" or "sound"

    void Start()
    {
        float initialValue = VolumeManager.GetVolume(volumeType);
        slider.value = initialValue;
        UpdateText(initialValue);

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        VolumeManager.SetVolume(value, volumeType);
        UpdateText(value);
    }

    void UpdateText(float value)
    {
        int percent = Mathf.RoundToInt(value * 100);
        valueText.text = percent + "%";
    }
}
