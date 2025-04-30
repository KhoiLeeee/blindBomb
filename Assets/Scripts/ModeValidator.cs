using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeValidator : MonoBehaviour
{
    public TMP_Dropdown playerNumDropdown;
    public TMP_Dropdown aiNumDropdown;
    public Button startButton;

    void Start()
    {
        playerNumDropdown.onValueChanged.AddListener(delegate { Validate(); });
        aiNumDropdown.onValueChanged.AddListener(delegate { Validate(); });

        Validate();
    }

    void Validate()
    {
        int playerNum = int.Parse(playerNumDropdown.options[playerNumDropdown.value].text);
        int aiNum = int.Parse(aiNumDropdown.options[aiNumDropdown.value].text);

        int total = playerNum + aiNum;
        startButton.interactable = (total > 1 && total <= 4);
    }
}
