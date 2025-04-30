using System.Collections.Generic;
using UnityEngine;

public static class KeyBindingRegistry
{
    public static Dictionary<string, KeyCode> Player1Keys = new();
    public static Dictionary<string, KeyCode> Player2Keys = new();

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