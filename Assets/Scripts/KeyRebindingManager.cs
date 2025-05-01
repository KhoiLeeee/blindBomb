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
            if (key.ToString().Contains("Joystick") || key.ToString().Contains("Mouse") || key.ToString().Contains("Keypad")) continue;
            used.Add(key);

            GameObject btnObj = Instantiate(keyButtonPrefab, gridParent);
            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();
            label.fontSize = 12;
            label.text = KeyCodeToSymbol(key);

            Button btn = btnObj.GetComponent<Button>();
            keyButtons[key] = btn;

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();

            var padding = new Vector4(5, 5, 5, 5);
            label.margin = padding;

            // Resize the button based on the label size
            float width = label.preferredWidth + padding.x + padding.y;
            float height = label.preferredHeight + padding.z + padding.w;
            btnRect.sizeDelta = new Vector2(width, height);
        }
    }
    private string KeyCodeToSymbol(KeyCode key)
    {
        switch (key)
        {
            // Arrow keys
            case KeyCode.LeftArrow: return "←";
            case KeyCode.RightArrow: return "→";
            case KeyCode.UpArrow: return "↑";
            case KeyCode.DownArrow: return "↓";

            // Common keys
            case KeyCode.Return: return "Enter";
            case KeyCode.Space: return "Space";
            case KeyCode.Escape: return "Esc";
            case KeyCode.Tab: return "Tab";

            // Modifier keys
            case KeyCode.LeftShift: return "LShift";
            case KeyCode.RightShift: return "RShift";
            case KeyCode.LeftControl: return "LCtrl";
            case KeyCode.RightControl: return "RCtrl";
            case KeyCode.LeftAlt: return "LAlt";
            case KeyCode.RightAlt: return "RAlt";

            // Function keys (F1 to F12)
            case KeyCode.F1: return "F1";
            case KeyCode.F2: return "F2";
            case KeyCode.F3: return "F3";
            case KeyCode.F4: return "F4";
            case KeyCode.F5: return "F5";
            case KeyCode.F6: return "F6";
            case KeyCode.F7: return "F7";
            case KeyCode.F8: return "F8";
            case KeyCode.F9: return "F9";
            case KeyCode.F10: return "F10";
            case KeyCode.F11: return "F11";
            case KeyCode.F12: return "F12";

            // Alphanumeric keys
            case KeyCode.A: return "A";
            case KeyCode.B: return "B";
            case KeyCode.C: return "C";
            case KeyCode.D: return "D";
            case KeyCode.E: return "E";
            case KeyCode.F: return "F";
            case KeyCode.G: return "G";
            case KeyCode.H: return "H";
            case KeyCode.I: return "I";
            case KeyCode.J: return "J";
            case KeyCode.K: return "K";
            case KeyCode.L: return "L";
            case KeyCode.M: return "M";
            case KeyCode.N: return "N";
            case KeyCode.O: return "O";
            case KeyCode.P: return "P";
            case KeyCode.Q: return "Q";
            case KeyCode.R: return "R";
            case KeyCode.S: return "S";
            case KeyCode.T: return "T";
            case KeyCode.U: return "U";
            case KeyCode.V: return "V";
            case KeyCode.W: return "W";
            case KeyCode.X: return "X";
            case KeyCode.Y: return "Y";
            case KeyCode.Z: return "Z";

            // Numbers and punctuation
            case KeyCode.Alpha0: return "0";
            case KeyCode.Alpha1: return "1";
            case KeyCode.Alpha2: return "2";
            case KeyCode.Alpha3: return "3";
            case KeyCode.Alpha4: return "4";
            case KeyCode.Alpha5: return "5";
            case KeyCode.Alpha6: return "6";
            case KeyCode.Alpha7: return "7";
            case KeyCode.Alpha8: return "8";
            case KeyCode.Alpha9: return "9";
            case KeyCode.Comma: return ",";
            case KeyCode.Period: return ".";
            case KeyCode.Slash: return "/";
            case KeyCode.Backslash: return "\\";

            //Operator
            case KeyCode.Minus: return "-";
            case KeyCode.Equals: return "=";
            case KeyCode.Plus: return "+";
            case KeyCode.Asterisk: return "*";
            case KeyCode.Semicolon: return ";";
            case KeyCode.Quote: return "\"";
            case KeyCode.LeftBracket: return "[";
            case KeyCode.RightBracket: return "]";
            case KeyCode.BackQuote: return "`";
            case KeyCode.Tilde: return "~";
            case KeyCode.Hash: return "#";
            case KeyCode.At: return "@";
            case KeyCode.Dollar: return "$";
            case KeyCode.Percent: return "%";
            case KeyCode.Caret: return "^";
            case KeyCode.Ampersand: return "&";
            case KeyCode.Underscore: return "_";
            case KeyCode.Colon: return ":";
            case KeyCode.Question: return "?";
            case KeyCode.Pipe: return "|";

            // Miscellaneous
            case KeyCode.Backspace: return "Backspace";
            case KeyCode.Delete: return "Del";
            case KeyCode.Insert: return "Ins";
            case KeyCode.Home: return "Home";
            case KeyCode.End: return "End";
            case KeyCode.PageUp: return "PgUp";
            case KeyCode.PageDown: return "PgDn";

            default: return key.ToString();
        }
    }

}
