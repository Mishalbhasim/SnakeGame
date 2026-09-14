using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a single food item at a random grid cell that isn't occupied by the snake.
/// Attach to an empty GameObject called "FoodSpawner" in the scene.
/// Assign the FoodItem prefab in the Inspector.
/// </summary>
public class FoodSpawner : MonoBehaviour
{
    [Tooltip("Prefab used to represent food on the grid")]
    public GameObject foodPrefab;

    public static FoodSpawner Instance { get; private set; }

    private GameObject currentFoodObject;

    /// <summary>
    /// The grid cell the current food occupies. SnakeController checks against this.
    /// </summary>
    public Vector2Int CurrentFoodPosition { get; private set; }

    /// <summary>
    /// True once the grid has no empty cells left to place food in (snake fills the board).
    /// GameManager listens for this to trigger a win state.
    /// </summary>
    public bool IsGridFull { get; private set; }

    // Fired when there's no empty cell left to spawn food - i.e. the player has won.
    public delegate void GridFull();
    public static event GridFull OnGridFull;

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
        // Spawn the first food with no occupied cells to avoid (snake not moved yet)
        SpawnFood(new List<Vector2Int>());
    }

    /// <summary>
    /// Called by GameManager on restart so a fresh game doesn't inherit a stale "grid full" state.
    /// </summary>
    public void ResetSpawner()
    {
        IsGridFull = false;
        SpawnFood(new List<Vector2Int>());
    }

    /// <summary>
    /// Spawns (or respawns) food at a random cell that is not in occupiedCells.
    /// Called at game start and every time the snake eats.
    /// </summary>
    public void SpawnFood(List<Vector2Int> occupiedCells)
    {
        int totalCells = GridSystem.Instance.width * GridSystem.Instance.height;

        // If every cell is occupied by the snake, there is nowhere left to place food.
        // This means the player has filled the entire grid - a win, not a bug.
        if (occupiedCells.Count >= totalCells)
        {
            IsGridFull = true;

            // Hide the food object since there's no valid cell for it anymore.
            if (currentFoodObject != null)
            {
                currentFoodObject.SetActive(false);
            }

            OnGridFull?.Invoke();
            return;
        }

        // Build the list of all empty cells directly rather than randomly retrying,
        // since random retries get slow/unreliable as the grid fills up.
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        for (int x = 0; x < GridSystem.Instance.width; x++)
        {
            for (int y = 0; y < GridSystem.Instance.height; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (!occupiedCells.Contains(cell))
                {
                    emptyCells.Add(cell);
                }
            }
        }

        Vector2Int newPos = emptyCells[Random.Range(0, emptyCells.Count)];
        CurrentFoodPosition = newPos;

        if (currentFoodObject == null)
        {
            currentFoodObject = Instantiate(foodPrefab, transform);
        }

        currentFoodObject.SetActive(true);
        currentFoodObject.transform.position = GridSystem.Instance.GridToWorld(newPos);
    }
}