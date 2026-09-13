using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the snake: grid-based movement on a fixed tick, growth on eating,
/// and collision detection (walls + self).
/// Attach to an empty GameObject called "Snake" in the scene.
/// Assign the SnakeSegment prefab in the Inspector.
/// </summary>
public class SnakeController : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Prefab used for every segment of the snake, including the head")]
    public GameObject segmentPrefab;

    [Tooltip("Starting length of the snake")]
    public int startLength = 3;

    [Tooltip("Starting grid position of the head")]
    public Vector2Int startPosition = new Vector2Int(10, 10);

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

    // Other scripts (GameManager) subscribe to these
    public delegate void FoodEaten();
    public static event FoodEaten OnFoodEaten;

    public delegate void SnakeDied();
    public static event SnakeDied OnSnakeDied;

    private void Start()
    {
        ResetSnake();
    }

    /// <summary>
    /// Resets the snake to its starting state. Called at game start and on restart.
    /// </summary>
    public void ResetSnake()
    {
        // Clean up any existing segments (used on restart)
        foreach (var seg in segments)
        {
            if (seg != null) Destroy(seg.gameObject);
        }
        segments.Clear();
        segmentGridPositions.Clear();

        direction = Vector2Int.right;
        pendingDirection = Vector2Int.right;
        isDead = false;
        moveTimer = 0f;

        // Build initial segments extending to the left of the start position
        for (int i = 0; i < startLength; i++)
        {
            Vector2Int gridPos = new Vector2Int(startPosition.x - i, startPosition.y);
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
        // Basic WASD / Arrow key input. Prevents reversing directly into itself.
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && direction != Vector2Int.down)
            pendingDirection = Vector2Int.up;
        else if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && direction != Vector2Int.up)
            pendingDirection = Vector2Int.down;
        else if ((Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) && direction != Vector2Int.right)
            pendingDirection = Vector2Int.left;
        else if ((Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) && direction != Vector2Int.left)
            pendingDirection = Vector2Int.right;
    }

    private void Move()
    {
        direction = pendingDirection;

        Vector2Int currentHeadPos = segmentGridPositions[0];
        Vector2Int newHeadPos = currentHeadPos + direction;

        // --- Wall collision ---
        if (!GridSystem.Instance.IsInsideGrid(newHeadPos))
        {
            Die();
            return;
        }

        // --- Self collision ---
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

    /// <summary>
    /// Checks whether there is food at the given grid position via FoodSpawner,
    /// and if so, tells the spawner to respawn it elsewhere.
    /// </summary>
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

    /// <summary>
    /// Exposes current occupied cells, e.g. so FoodSpawner avoids spawning on the snake.
    /// </summary>
    public List<Vector2Int> GetOccupiedCells()
    {
        return segmentGridPositions;
    }
}