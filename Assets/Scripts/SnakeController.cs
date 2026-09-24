using System.Collections.Generic;
using UnityEngine;


public class SnakeController : MonoBehaviour
{
    public static SnakeController Instance { get; private set; }

    public int CurrentLength => segments.Count;

    [Header("Setup")]
    [Tooltip("Prefab used for every segment of the snake, including the head")]
    public GameObject segmentPrefab;

    [Tooltip("Starting length of the snake")]
    public int startLength = 3;

    [Tooltip("Starting grid position of the head. If left as (0,0), the snake auto-centers on the grid instead.")]
    public Vector2Int startPosition = Vector2Int.zero;

    [Header("Movement")]
    [Tooltip("Seconds between each grid move. Lower = faster snake.")]
    public float moveInterval = 0.15f;

    // Internal state
    private List<Transform> segments = new List<Transform>();
    private List<Vector2Int> segmentGridPositions = new List<Vector2Int>();

    private Vector2Int direction = Vector2Int.right;
    private Vector2Int pendingDirection = Vector2Int.right;

    private float moveTimer = 0f;
    private bool isDead = false;

    
    public delegate void FoodEaten();
    public static event FoodEaten OnFoodEaten;

    public delegate void SnakeDied();
    public static event SnakeDied OnSnakeDied;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ResetSnake();
    }

    public void ResetSnake()
    {
        ClearSegments();

        direction = Vector2Int.right;
        pendingDirection = Vector2Int.right;
        isDead = false;
        moveTimer = 0f;

        BuildBodyAtSafeSpawn();
    }


    
    public void Revive()
    {
        isDead = false;

        Vector2Int head = segmentGridPositions[0];
        Vector2Int[] candidates =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (Vector2Int candidate in candidates)
        {
            if (candidate == -direction) continue; 

            Vector2Int next = head + candidate;
            if (!GridSystem.Instance.IsInsideGrid(next)) continue;
            if (segmentGridPositions.Contains(next)) continue;

            direction = candidate;
            pendingDirection = candidate;
            break;
        }

        moveTimer = -0.5f;
    }

    private void ClearSegments()
    {
        foreach (var seg in segments)
        {
            if (seg != null) Destroy(seg.gameObject);
        }
        segments.Clear();
        segmentGridPositions.Clear();
    }

    private void BuildBodyAtSafeSpawn()
    {
      
        Vector2Int spawnPos = startPosition;
        if (spawnPos == Vector2Int.zero)
        {
            spawnPos = new Vector2Int(GridSystem.Instance.width / 2, GridSystem.Instance.height / 2);
        }

        for (int i = 0; i < startLength; i++)
        {
            Vector2Int gridPos = new Vector2Int(spawnPos.x - i, spawnPos.y);
            GameObject segObj = Instantiate(segmentPrefab, transform);
            segObj.transform.position = GridSystem.Instance.GridToWorld(gridPos);

            segments.Add(segObj.transform);
            segmentGridPositions.Add(gridPos);
        }
    }

    private void Update()
    {
        if (isDead) return;

        HandleInput();

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Move();
        }
    }

    private void HandleInput()
    {
    
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && direction != Vector2Int.down)
            pendingDirection = Vector2Int.up;
        else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && direction != Vector2Int.up)
            pendingDirection = Vector2Int.down;
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && direction != Vector2Int.right)
            pendingDirection = Vector2Int.left;
        else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && direction != Vector2Int.left)
            pendingDirection = Vector2Int.right;
    }



    public void OnUpPressed()
    {
        if (direction != Vector2Int.down) pendingDirection = Vector2Int.up;
    }

    public void OnDownPressed()
    {
        if (direction != Vector2Int.up) pendingDirection = Vector2Int.down;
    }

    public void OnLeftPressed()
    {
        if (direction != Vector2Int.right) pendingDirection = Vector2Int.left;
    }

    public void OnRightPressed()
    {
        if (direction != Vector2Int.left) pendingDirection = Vector2Int.right;
    }

    private void Move()
    {
        direction = pendingDirection;

        Vector2Int currentHeadPos = segmentGridPositions[0];
        Vector2Int newHeadPos = currentHeadPos + direction;

        //Wall collision
        if (!GridSystem.Instance.IsInsideGrid(newHeadPos))
        {
            Die();
            return;
        }

        // Self collision 
        for (int i = 0; i < segmentGridPositions.Count; i++)
        {
            if (segmentGridPositions[i] == newHeadPos)
            {
                Die();
                return;
            }
        }

        bool ateFood = CheckFoodAt(newHeadPos);

        // Insert new head position at the front of the data list
        segmentGridPositions.Insert(0, newHeadPos);

        if (!ateFood)
        {
            // Not growing: drop the tail position
            segmentGridPositions.RemoveAt(segmentGridPositions.Count - 1);
        }
        else
        {
            // Growing: add a new segment transform (its position gets set below)
            GameObject newSeg = Instantiate(segmentPrefab, transform);
            segments.Add(newSeg.transform);
            OnFoodEaten?.Invoke();
        }

        // Apply world positions to all segment transforms based on grid positions
        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].position = GridSystem.Instance.GridToWorld(segmentGridPositions[i]);
        }
    }


    private bool CheckFoodAt(Vector2Int gridPos)
    {
        if (FoodSpawner.Instance == null) return false;

        if (FoodSpawner.Instance.CurrentFoodPosition == gridPos)
        {
            FoodSpawner.Instance.SpawnFood(segmentGridPositions);
            return true;
        }
        return false;
    }

    private void Die()
    {
        isDead = true;
        OnSnakeDied?.Invoke();
    }


    public List<Vector2Int> GetOccupiedCells()
    {
        return segmentGridPositions;
    }
}