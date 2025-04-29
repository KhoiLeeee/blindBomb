using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 direction = Vector2.down;
    private Vector2 moveTargetPos;
    public float speed = 5f;
    private Vector2Int startCell;
    private Vector2Int targetCell;
    private bool isMoving = false;


    [Header("Input")]
    public KeyCode inputUp = KeyCode.W;
    public KeyCode inputDown = KeyCode.S;
    public KeyCode inputLeft = KeyCode.A;
    public KeyCode inputRight = KeyCode.D;

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
    }

    private void Update()
    {
        //List<GameObject> bombs = GameManager.GetAllBombs();

        //string bombS = "";
        //foreach(GameObject bomb in bombs)
        //{
        //    if (bomb == null) continue;
        //    Vector2Int pos = new Vector2Int((int)bomb.transform.position.x, (int)bomb.transform.position.y);
        //    Tuple<int, int> tup = MapManager.Instance.Vec2IntToGridBased(pos);
        //    string s = "(" + tup.Item1.ToString() + ":" + tup.Item2.ToString() + "),";
        //    bombS += s;
        //}
        //Debug.Log(bombS);
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

        //if (!isMoving)
        //{
        //    if (Input.GetKey(inputUp))
        //    {
        //        SetDirection(Vector2.up, spriteRendererUp);
        //    }
        //    else if (Input.GetKey(inputDown))
        //    {
        //        SetDirection(Vector2.down, spriteRendererDown);
        //    }
        //    else if (Input.GetKey(inputLeft))
        //    {
        //        SetDirection(Vector2.left, spriteRendererLeft);
        //    }
        //    else if (Input.GetKey(inputRight))
        //    {
        //        SetDirection(Vector2.right, spriteRendererRight);
        //    }
        //    else
        //    {
        //        SetDirection(Vector2.zero, activeSpriteRenderer);
        //    }


        //    if (direction != Vector2.zero)
        //    {
        //        startCell = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        //        Vector2Int direct = new Vector2Int((int)direction.x, (int)direction.y);
        //        targetCell = startCell + direct;

        //        if (MapManager.Instance.IsInsideMap(targetCell) && MapManager.Instance.GetCell(targetCell) == CellType.Empty)
        //        {
        //            MapManager.Instance.SetCell(startCell, CellType.Empty);

        //            moveTargetPos = new Vector2(targetCell.x + 0.5f, targetCell.y + 0.5f);
        //            isMoving = true;
        //        }
        //    }
        //}
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
        //if (newCell != startCell)
        //{
        //    if (MapManager.Instance.GetCell(startCell) != CellType.Bomb)
        //    {
        //        MapManager.Instance.SetCell(startCell, CellType.Empty);
        //    }
            
        //    MapManager.Instance.SetCell(newCell, CellType.Player);
        //    startCell = newCell;
        //}
        rb.MovePosition(newPosition);
        //if (isMoving)
        //{
        //    rb.MovePosition(Vector2.MoveTowards(rb.position, moveTargetPos, speed * Time.fixedDeltaTime));

        //    if (Vector2.Distance(rb.position, moveTargetPos) < 0.01f)
        //    {
        //        MapManager.Instance.SetCell(targetCell, CellType.Player);

        //        rb.position = moveTargetPos;
        //        isMoving = false;
        //    }
        //}
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
        GameManager.Instance.CheckWinState();
    }
}
