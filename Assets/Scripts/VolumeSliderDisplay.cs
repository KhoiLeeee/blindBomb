using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeSliderDisplay : MonoBehaviour
{
    public Slider slider;
    public TMP_Text valueText; // Or TMP_Text if using TextMeshPro

    void Start()
    {
        slider.onValueChanged.AddListener(UpdateText);
        UpdateText(slider.value);
    }

    void UpdateText(float value)
    {
        valueText.text = Mathf.RoundToInt(value).ToString();
    }
}
