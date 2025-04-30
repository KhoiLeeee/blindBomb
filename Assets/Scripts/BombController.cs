using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombController : MonoBehaviour
{

    private string agentName;

    [Header("Bomb")]
    public KeyCode inputKey = KeyCode.LeftShift;
    public GameObject bombPrefab;
    public float bombFuseTime = 4f;
    public int bombAmount = 1;
    private int bombsRemaining;

    [Header("Explosion")]
    public Explosion explosionPrefab;
    public LayerMask explosionLayerMask;
    public float explosionDuration = 0.5f;
    public int explosionRadius = 1;

    [Header("Destructible")]
    public Tilemap destructibleTiles;

    [System.Serializable]
    public class DestructibleMapping
    {
        public TileBase tile;
        public Destructible prefab;
    }
    public List<DestructibleMapping> destructibleMappings;

    private void OnEnable()
    {
        bombsRemaining = bombAmount;
        agentName = gameObject.name;
    }

    private void Update()
    {
        if (bombsRemaining > 0 && Input.GetKeyDown(inputKey))
        {
            StartCoroutine(PlaceBomb());
        }
    }

    private IEnumerator PlaceBomb()
    {
        Vector2 position = transform.position;
        position.x = Mathf.Floor(position.x) + 0.5f;
        position.y = Mathf.Floor(position.y) + 0.5f;

        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        GameManager.AddBombs(agentName, bomb);

        Bomb bombComponent = bomb.GetComponent<Bomb>();

        if (bombComponent == null)
        {
            bombComponent = bomb.AddComponent<Bomb>();
        }
        bombComponent.Initialize(bombFuseTime, explosionRadius); // ExplosionRadius can be random
        bombsRemaining--;
        // Set Bomb Cell in MapGrid
        Vector2Int cell = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        MapManager.Instance.SetCell(cell, CellType.Bomb);

        yield return new WaitForSeconds(bombFuseTime);
        Debug.Log("Bomb explode");
        position = bomb.transform.position;
        position.x = Mathf.Floor(position.x) + 0.5f;
        position.y = Mathf.Floor(position.y) + 0.5f;
        // Set Empty Cell in MapGrid when exploding
        MapManager.Instance.SetCell(cell, CellType.Empty);

        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(explosion.start);
        explosion.DestroyAfter(explosionDuration);

        Explode(position, Vector2.up, explosionRadius);
        Explode(position, Vector2.down, explosionRadius);
        Explode(position, Vector2.left, explosionRadius);
        Explode(position, Vector2.right, explosionRadius);

        Destroy(bomb);
        GameManager.RemoveBomb(agentName, bomb);

        bombsRemaining++;
    }

    private void Explode(Vector2 position, Vector2 direction, int length)
    {
        if (length <= 0)
        {
            return;
        }

        position += direction;

        // Compute position of this cell
        Vector2Int cell = new Vector2Int(
            Mathf.FloorToInt(position.x),
            Mathf.FloorToInt(position.y)
        );

        if (Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask))
        {
            ClearDestructible(position);
            return;
        }

        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(length > 1 ? explosion.middle : explosion.end);
        // explosion.SetDirection(direction);
        explosion.DestroyAfter(explosionDuration);

        // Set Explosion Cell and clear after delay
        MapManager.Instance.SetCell(cell, CellType.Explosion);
        StartCoroutine(ClearExplosionAfterDelay(cell, explosionDuration));

        Explode(position, direction, length - 1);
    }
    private IEnumerator ClearExplosionAfterDelay(Vector2Int cell, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (MapManager.Instance.GetCell(cell) == CellType.Explosion)
        {
            MapManager.Instance.SetCell(cell, CellType.Empty);
        }
    }

    private void ClearDestructible(Vector2 position)
    {
        Vector3Int cell = destructibleTiles.WorldToCell(position);
        TileBase tile = destructibleTiles.GetTile(cell);

        if (tile != null)
        {
            foreach (DestructibleMapping mapping in destructibleMappings)
            {
                if (mapping.tile == tile)
                {
                    Instantiate(mapping.prefab, position, Quaternion.identity);
                    destructibleTiles.SetTile(cell, null);
                    Vector2Int block = new Vector2Int(cell.x, cell.y);
                    MapManager.Instance.SetCell(block, CellType.Empty);
                    break;
                }
            }
        }
    }

    public void AddBomb()
    {
        bombAmount++;
        bombsRemaining++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bomb"))
        {
            other.isTrigger = false;
        }
    }

    public int getBombRemaining()
    {
        return this.bombsRemaining;
    }

    public void PlaceBombManually()
    {
        if (bombsRemaining > 0)
        {
            StartCoroutine(PlaceBomb());
        }
    }
}
