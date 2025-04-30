using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Manager : MonoBehaviour
{
    public enum MapType { Fixed, Random }
    public enum GameMode { OneVsOne, TwoVsTwo, OneVsThree }

    public MapType selectedMapType;
    public GameMode selectedGameMode;

    public GameObject[] players; // 0-3
    public Transform[] spawnPoints; // fixed positions for 4 players

    private List<GameObject> teamA = new List<GameObject>();
    private List<GameObject> teamB = new List<GameObject>();

    public GameObject pauseGameScreen;
    public GameObject continueButton;
    public GameObject backToMenuButton;

    public Tilemap destructibleTiles;
    public Tilemap indestructibleTiles;

    public Tile barrelTile;
    public Tile boxTile;
    public Tile signTile;

    public int barrelCount = 10;
    public int boxCount = 10;
    public int signCount = 10;

    void Start()
    {
        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }
        SetupTeams();
        SpawnPlayers();

        if (selectedMapType == MapType.Random)
        {
            ClearDestructibles();
            Debug.Log("Generating random map...");
            GenerateRandomMap();
        }
        else
        {
            LoadFixedMap();
        }
    }

    void Update()
    {
        CheckWinCondition();
    }

    void SetupTeams()
    {
        teamA.Clear();
        teamB.Clear();

        if (selectedGameMode == GameMode.OneVsOne)
        {
            teamA.Add(players[0]);
            teamB.Add(players[3]);
        }
        else if(selectedGameMode == GameMode.TwoVsTwo) // 2v2
        {
            teamA.Add(players[0]);
            teamA.Add(players[1]);
            teamB.Add(players[2]);
            teamB.Add(players[3]);
        }
        else
        {
            teamA.Add(players[0]);
            teamB.Add(players[1]);
            teamB.Add(players[2]);
            teamB.Add(players[3]);
        }
    }

    void SpawnPlayers()
    {
        int numPlayers = 4;
        if (selectedGameMode == GameMode.OneVsOne)
        {
            numPlayers = 2;
        }
        for (int i = 0; i < numPlayers; i++)
        {
            players[i].SetActive(true);
            players[i].transform.position = spawnPoints[i].position;
        }
    }

    void CheckWinCondition()
    {
        teamA.RemoveAll(player => player == null);
        teamB.RemoveAll(player => player == null);

        if (teamA.Count == 0)
        {
            EndGame("Team B Wins!");
        }
        else if (teamB.Count == 0)
        {
            EndGame("Team A Wins!");
        }
    }

    void EndGame(string message)
    {
        Debug.Log(message);
        // Show UI panel with "Play Again" / "Back to Menu"
        Time.timeScale = 0f; // pause game
    }

    void ClearDestructibles()
    {
        destructibleTiles.ClearAllTiles();
    }

    List<Vector3Int> GetAvailablePositions()
    {
        List<Vector3Int> positions = new List<Vector3Int>();

        int xMin = -8;
        int xMax = 8;   // exclusive (goes from -8 to +7)
        int yMin = -4;
        int yMax = 4;   // exclusive (goes from -4 to +3)

        HashSet<Vector3Int> playerCells = new HashSet<Vector3Int>();
        foreach (GameObject player in players)
        {
            Vector3Int cell = destructibleTiles.WorldToCell(player.transform.position);
            playerCells.Add(cell);
            playerCells.Add(cell + Vector3Int.up);
            playerCells.Add(cell + Vector3Int.down);
            playerCells.Add(cell + Vector3Int.left);
            playerCells.Add(cell + Vector3Int.right);
        }

        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (destructibleTiles.GetTile(pos) == null &&
                    indestructibleTiles.GetTile(pos) == null &&
                    !playerCells.Contains(pos))
                {
                    positions.Add(pos);
                }
            }
        }

        Debug.Log("Available positions: " + positions.Count);
        return positions;
    }

    void GenerateRandomMap()
    {
        List<Vector3Int> positions = GetAvailablePositions();
        System.Random rng = new System.Random();
        positions = positions.OrderBy(x => rng.Next()).ToList(); // shuffle

        int total = barrelCount + boxCount + signCount;
        int placed = 0;

        for (int i = 0; i < total && i < positions.Count; i++)
        {
            Vector3Int cell = positions[i];

            Tile tileToPlace = null;

            if (placed < barrelCount) tileToPlace = barrelTile;
            else if (placed < barrelCount + boxCount) tileToPlace = boxTile;
            else tileToPlace = signTile;

            destructibleTiles.SetTile(cell, tileToPlace);
            placed++;
        }
    }
    void LoadFixedMap() { /* your logic here */ }

    public void onPauseButtonClicked()
    {
        pauseGameScreen.SetActive(true);
        Time.timeScale = 0f;
    }
    public void onContinueButtonClicked()
    {
        Time.timeScale = 1f;
        pauseGameScreen.SetActive(false);
    }
    
    public void onMenuButtonClicked()
    {
        SceneManager.LoadScene("mainMenu");
    }
}
