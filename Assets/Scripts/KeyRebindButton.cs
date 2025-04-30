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
    }

    public void OnButtonClick()
    {
        KeyCode currentKey = KeyBindingRegistry.GetKey(playerID, actionName);
        rebindingManager.OpenPopup(playerID, actionName, currentKey, keyLabel);
    }
}
