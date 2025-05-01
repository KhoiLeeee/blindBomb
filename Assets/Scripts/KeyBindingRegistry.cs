using System.Collections.Generic;
using UnityEngine;

public static class KeyBindingRegistry
{
    public static Dictionary<string, KeyCode> Player1Keys = new();
    public static Dictionary<string, KeyCode> Player2Keys = new();

    static KeyBindingRegistry()
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
    public static HashSet<KeyCode> GetUnavailableKeys(string currentPlayer)
    {
        HashSet<KeyCode> used = new();
        foreach (var key in Player1Keys.Values)
            used.Add(key);
        foreach( var key in Player2Keys.Values)
            used.Add(key);

        if (currentPlayer == "Player 1")
            foreach (var key in Player1Keys.Values)
                used.Remove(key);
        else if (currentPlayer == "Player 2")
            foreach (var key in Player2Keys.Values)
                used.Remove(key);

        return used;
    }

    public static void UpdateKey(string player, string actionName, KeyCode newKey)
    {
        if (player == "Player 1")
            Player1Keys[actionName] = newKey;
        else
            Player2Keys[actionName] = newKey;
    }

    public static KeyCode GetKey(string player, string actionName)
    {
        if (player == "Player 1")
            return Player1Keys[actionName];
        else
            return Player2Keys[actionName];
    }
}