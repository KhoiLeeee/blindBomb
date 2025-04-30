using System.Data;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public float destructionTime = 1f;
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.9f;
    public GameObject[] spawnableItems;

    private void Start()
    {
        Destroy(gameObject, destructionTime);
    }

    private void OnDestroy()
    {
        if (spawnableItems.Length > 0 && Random.value < itemSpawnChance)
        {
            int randomIndex = Random.Range(0, spawnableItems.Length);
            GameObject spawnedItem = Instantiate(spawnableItems[randomIndex], transform.position, Quaternion.identity);
            //Vector2Int cell = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));

            GameManager.AddCoins(spawnedItem);
        }
    }

}
