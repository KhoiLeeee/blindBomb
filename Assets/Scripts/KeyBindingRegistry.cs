using System.Collections.Generic;
using UnityEngine;

public class KeyBindingRegistry : MonoBehaviour
{
    public static KeyBindingRegistry Instance;

    public Dictionary<string, KeyCode> Player1Keys;
    public Dictionary<string, KeyCode> Player2Keys;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Player1Keys = new Dictionary<string, KeyCode>();
            Player2Keys = new Dictionary<string, KeyCode>();

            InitializeDefaults();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //private void Update()
    //{
    //    if (Player1Keys.Count == 5)
    //    {
    //        Debug.Log("Player 1");
    //        Debug.Log(Player1Keys["Up"]);
    //        Debug.Log(Player1Keys["Down"]);
    //        Debug.Log(Player1Keys["Left"]);
    //        Debug.Log(Player1Keys["Right"]);
    //        Debug.Log(Player1Keys["Bomb"]);
    //    }

    //    if (Player2Keys.Count == 5)
    //    {
    //        Debug.Log("Player 2");
    //        Debug.Log(Player2Keys["Up"]);
    //        Debug.Log(Player2Keys["Down"]);
    //        Debug.Log(Player2Keys["Left"]);
    //        Debug.Log(Player2Keys["Right"]);
    //        Debug.Log(Player2Keys["Bomb"]);
    //    }
    //}
    private void InitializeDefaults()
    {

        if (Player1Keys.Count == 0)
        {
            Player1Keys.Add("Up", KeyCode.W);
            Player1Keys.Add("Down", KeyCode.S);
            Player1Keys.Add("Left", KeyCode.A);
            Player1Keys.Add("Right", KeyCode.D);
            Player1Keys.Add("Bomb", KeyCode.Space);
        }
        if (Player2Keys.Count == 0)
        {
            Player2Keys.Add("Up", KeyCode.UpArrow);
            Player2Keys.Add("Down", KeyCode.DownArrow);
            Player2Keys.Add("Left", KeyCode.LeftArrow);
            Player2Keys.Add("Right", KeyCode.RightArrow);
            Player2Keys.Add("Bomb", KeyCode.Return);
        }
    }
    public HashSet<KeyCode> GetUnavailableKeys(string currentPlayer)
    {
        HashSet<KeyCode> used = new(Player1Keys.Values);
        used.UnionWith(Player2Keys.Values);

        if (currentPlayer == "Player 1")
            foreach (var key in Player1Keys.Values) used.Remove(key);
        else if (currentPlayer == "Player 2")
            foreach (var key in Player2Keys.Values) used.Remove(key);

        return used;
    }

    public void UpdateKey(string player, string actionName, KeyCode newKey)
    {
        if (player == "Player 1")
            Player1Keys[actionName] = newKey;
        else
            Player2Keys[actionName] = newKey;
    }

    public KeyCode GetKey(string player, string actionName)
    {
        if (player == "Player 1")
            return Player1Keys[actionName];
        else
            return Player2Keys[actionName];
    }
}