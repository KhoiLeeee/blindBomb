using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KeyRebindButton : MonoBehaviour
{
    public string playerID;
    public string actionName;
    public TMP_Text keyLabel;

    private KeyRebindingManager rebindingManager;

    private void Start()
    {
        rebindingManager = KeyRebindingManager.Instance;
        GetComponent<Button>().onClick.AddListener(OnButtonClick);

        KeyCode currentKey = KeyBindingRegistry.Instance.GetKey(playerID, actionName);
        keyLabel.text = KeyRebindingManager.Instance.KeyCodeToSymbol(currentKey);

    }

    public void OnButtonClick()
    {
        KeyCode currentKey = KeyBindingRegistry.Instance.GetKey(playerID, actionName);
        rebindingManager.OpenPopup(playerID, actionName, currentKey, keyLabel);
    }
}
