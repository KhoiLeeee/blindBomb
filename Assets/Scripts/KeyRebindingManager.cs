using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class KeyRebindingManager : MonoBehaviour
{
    public static KeyRebindingManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject popupPanel;
    public Transform gridParent;
    public TMP_Text currentKeyText;
    public GameObject keyButtonPrefab;

    [Header("Internal State")]
    private string currentPlayer;
    private string actionName;
    private Dictionary<KeyCode, Button> keyButtons = new Dictionary<KeyCode, Button>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CreateKeyButtons();
    }
    public void OpenPopup(string _player, string _actionName, KeyCode currentKey, TMP_Text keyTextLabel)
    {
        popupPanel.SetActive(true);

        currentPlayer = _player;
        actionName = _actionName;
        currentKeyText = keyTextLabel;

        HashSet<KeyCode> unavailable = KeyBindingRegistry.GetUnavailableKeys(currentPlayer);

        foreach (var pair in keyButtons)
        {
            KeyCode btnKey = pair.Key;
            Button button = pair.Value;
            TMP_Text btnText = button.GetComponentInChildren<TMP_Text>();

            ColorBlock colors = button.colors;
            if (btnKey == currentKey)
            {
                colors.normalColor = Color.green;
                button.interactable = true;
            }
            else if (unavailable.Contains(btnKey))
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
            button.onClick.AddListener(() => OnKeySelected(btnKey)); 
        }
    }

    public void OnKeySelected(KeyCode selectedKey)
    {
        KeyBindingRegistry.UpdateKey(currentPlayer, actionName, selectedKey);
        currentKeyText.text = KeyCodeToSymbol(selectedKey);
        popupPanel.SetActive(false);
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    private void CreateKeyButtons()
    {
        KeyCode[] allKeys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
        HashSet<KeyCode> used = new HashSet<KeyCode>();

        foreach (KeyCode key in allKeys)
        {
            if ((int)key < (int)KeyCode.Space || used.Contains(key)) continue;

            used.Add(key);

            GameObject btnObj = Instantiate(keyButtonPrefab, gridParent);
            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();
            label.text = KeyCodeToSymbol(key);

            Button btn = btnObj.GetComponent<Button>();
            keyButtons[key] = btn;
        }
    }
    private string KeyCodeToSymbol(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.LeftArrow: return "←";
            case KeyCode.RightArrow: return "→";
            case KeyCode.UpArrow: return "↑";
            case KeyCode.DownArrow: return "↓";
            case KeyCode.Return: return "Enter";
            case KeyCode.Space: return "Space";
            default: return key.ToString().ToUpper();
        }
    }
}
