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
    /// Spawns (or respawns) food at a random cell that is not in occupiedCells.
    /// Called at game start and every time the snake eats.
    /// </summary>
    public void SpawnFood(List<Vector2Int> occupiedCells)
    {
        Vector2Int newPos;
        int safetyCounter = 0;

        do
        {
            newPos = GridSystem.Instance.GetRandomCell();
            safetyCounter++;
            // Safety valve: if the grid is nearly full, stop trying after many attempts
            // rather than looping forever.
        }
        while (occupiedCells.Contains(newPos) && safetyCounter < 1000);

        CurrentFoodPosition = newPos;

        if (currentFoodObject == null)
        {
            currentFoodObject = Instantiate(foodPrefab, transform);
        }

        currentFoodObject.transform.position = GridSystem.Instance.GridToWorld(newPos);
    }
}