using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyRebindingManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public Button[] keyButtons;
    public Text currentKeyText;

    [Header("Internal State")]
    private string currentSymbol;
    private Dictionary<string, KeyCode> symbolToKey;
    private Dictionary<KeyCode, string> keyToSymbol;
    private HashSet<KeyCode> unavailableKeys = new HashSet<KeyCode>();

    private void Awake()
    {
        InitializeKeyMappings();
    }

    private void InitializeKeyMappings()
    {
        symbolToKey = new Dictionary<string, KeyCode>
        {
            { "W", KeyCode.W },
            { "A", KeyCode.A },
            { "S", KeyCode.S },
            { "D", KeyCode.D },
            { "←", KeyCode.LeftArrow },
            { "→", KeyCode.RightArrow },
            { "↑", KeyCode.UpArrow },
            { "↓", KeyCode.DownArrow },
            { "Enter", KeyCode.Return },
            { "Space", KeyCode.Space }
        };

        keyToSymbol = new Dictionary<KeyCode, string>();
        foreach (var pair in symbolToKey)
            keyToSymbol[pair.Value] = pair.Key;
    }
    public void OpenPopup(string currentKeySymbol)
    {
        popupPanel.SetActive(true);
        currentSymbol = currentKeySymbol;

        KeyCode currentKey = symbolToKey.ContainsKey(currentKeySymbol)
            ? symbolToKey[currentKeySymbol]
            : KeyCode.None;

        foreach (Button button in keyButtons)
        {
            Text btnText = button.GetComponentInChildren<Text>();
            string btnSymbol = btnText.text;
            KeyCode btnKey = symbolToKey.ContainsKey(btnSymbol) ? symbolToKey[btnSymbol] : KeyCode.None;

            ColorBlock colors = button.colors;
            if (btnKey == currentKey)
            {
                colors.normalColor = Color.green;
                button.interactable = true;
            }
            else if (unavailableKeys.Contains(btnKey))
            {
                colors.normalColor = Color.gray;
                button.interactable = false;
            }
            else
            {
                colors.normalColor = Color.white;
                button.interactable = true;
            }

            button.colors = colors;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnKeySelected(btnSymbol)); 
        }
    }

    public void OnKeySelected(string selectedSymbol)
    {
        if (!symbolToKey.ContainsKey(selectedSymbol))
        {
            Debug.LogWarning("Unknown symbol selected: " + selectedSymbol);
            return;
        }

        KeyCode selectedKey = symbolToKey[selectedSymbol];
        KeyCode previousKey = symbolToKey[currentSymbol];

        unavailableKeys.Remove(previousKey);
        unavailableKeys.Add(selectedKey);

        currentKeyText.text = selectedSymbol;

        popupPanel.SetActive(false);
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
}
