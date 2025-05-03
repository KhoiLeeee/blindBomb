using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType
    {
        ExtraBomb,
        BlastRadius,
        SpeedIncrease,
    }

    public ItemType type;

    private void OnItemPickup(GameObject player)
    {
        switch (type)
        {
            case ItemType.ExtraBomb:
                player.GetComponent<BombController>().AddBomb();
                break;

            case ItemType.BlastRadius:
                player.GetComponent<BombController>().explosionRadiusMin++;
                player.GetComponent<BombController>().explosionRadiusMax++;
                break;

            case ItemType.SpeedIncrease:
                if (player.tag == "Player")
                {
                    player.GetComponent<MovementController>().speed++;
                }
                else
                {
                    player.GetComponent<RuleBasedAgent>().speed++;
                }
                break;
        }
        SoundEffects.Instance.PlaySound("PickUp");
        GameManager.RemoveCoin(gameObject);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            OnItemPickup(other.gameObject);
        }
    }

}
