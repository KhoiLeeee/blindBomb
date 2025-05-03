using UnityEngine;

public class Destructible : MonoBehaviour
{
    public float destructionTime = 1f;
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.25f;
    public GameObject[] spawnableItems;

    private void Start()
    {
        Destroy(gameObject, destructionTime);
    }

    private void OnDestroy()
    {
        float randomNum = Random.value;
        Debug.Log(randomNum);
        if (spawnableItems.Length > 0 && randomNum < itemSpawnChance)
        {
            int randomIndex = Random.Range(0, spawnableItems.Length);
            GameObject spawnedItem = Instantiate(spawnableItems[randomIndex], transform.position, Quaternion.identity);
            //Vector2Int cell = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));

            GameManager.AddCoins(spawnedItem);
        }
    }

}
