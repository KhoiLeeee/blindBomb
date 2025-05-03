using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direction = Vector2.down;
    private Vector2 moveTargetPos;
    public float speed = 2f;
    private Vector2Int startCell;
    private Vector2Int targetCell;
    public string playerID;

    [Header("Input")]
    private KeyCode inputUp = KeyCode.W;
    private KeyCode inputDown = KeyCode.S;
    private KeyCode inputLeft = KeyCode.A;
    private KeyCode inputRight = KeyCode.D;

    [Header("Sprites")]
    public AnimatedSpriteRenderer spriteRendererUp;
    public AnimatedSpriteRenderer spriteRendererDown;
    public AnimatedSpriteRenderer spriteRendererLeft;
    public AnimatedSpriteRenderer spriteRendererRight;
    public AnimatedSpriteRenderer spriteRendererDeath;
    private AnimatedSpriteRenderer activeSpriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        activeSpriteRenderer = spriteRendererDown;
        AssignInputKey();
    }

    private void AssignInputKey()
    {
        Dictionary<string, KeyCode> keyPair;
        switch (playerID)
        {
            case "Player 1":
                keyPair  = KeyBindingRegistry.Instance.Player1Keys;
                break;
            case "Player 2":
                keyPair = KeyBindingRegistry.Instance.Player2Keys;
                break;
            default:
                keyPair = KeyBindingRegistry.Instance.Player1Keys;
                break;
        }

        inputUp = keyPair["Up"];
        inputDown = keyPair["Down"];
        inputLeft = keyPair["Left"];
        inputRight = keyPair["Right"];
    }
    private void Update()
    {
        List<GameObject> coins = GameManager.GetCoins();

        if (Input.GetKey(inputUp))
        {
            SetDirection(Vector2.up, spriteRendererUp);
        }
        else if (Input.GetKey(inputDown))
        {
            SetDirection(Vector2.down, spriteRendererDown);
        }
        else if (Input.GetKey(inputLeft))
        {
            SetDirection(Vector2.left, spriteRendererLeft);
        }
        else if (Input.GetKey(inputRight))
        {
            SetDirection(Vector2.right, spriteRendererRight);
        }
        else
        {
            SetDirection(Vector2.zero, activeSpriteRenderer);
        }
    }

    private void FixedUpdate()
    {
        Vector2 position = rb.position;
        Vector2 translation = speed * Time.fixedDeltaTime * direction;
        Vector2 newPosition = position + translation;

        Vector2Int newCell = new Vector2Int(
            Mathf.FloorToInt(newPosition.x),
            Mathf.FloorToInt(newPosition.y)
        );
        if (newCell != startCell)
        {
            if (MapManager.Instance.GetCell(startCell) != CellType.Bomb)
            {
                MapManager.Instance.SetCell(startCell, CellType.Empty);
            }

            MapManager.Instance.SetCell(newCell, CellType.Player);
            startCell = newCell;
        }
        rb.MovePosition(newPosition);
    }

    private void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer)
    {
        direction = newDirection;

        spriteRendererUp.enabled = spriteRenderer == spriteRendererUp;
        spriteRendererDown.enabled = spriteRenderer == spriteRendererDown;
        spriteRendererLeft.enabled = spriteRenderer == spriteRendererLeft;
        spriteRendererRight.enabled = spriteRenderer == spriteRendererRight;

        activeSpriteRenderer = spriteRenderer;
        activeSpriteRenderer.idle = direction == Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            DeathSequence();
        }
    }

    private void DeathSequence()
    {
        enabled = false;
        GetComponent<BombController>().enabled = false;

        spriteRendererUp.enabled = false;
        spriteRendererDown.enabled = false;
        spriteRendererLeft.enabled = false;
        spriteRendererRight.enabled = false;
        spriteRendererDeath.enabled = true;

        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }

    private void OnDeathSequenceEnded()
    {
        gameObject.SetActive(false);
        Manager.Instance.CheckWinCondition();
    }
}
