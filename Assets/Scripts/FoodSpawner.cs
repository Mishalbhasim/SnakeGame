using System.Collections.Generic;
using UnityEngine;


public class FoodSpawner : MonoBehaviour
{
    [Tooltip("Prefab used to represent food on the grid")]
    public GameObject foodPrefab;

    public static FoodSpawner Instance { get; private set; }

    private GameObject currentFoodObject;

    
    public Vector2Int CurrentFoodPosition { get; private set; }

   
    public bool IsGridFull { get; private set; }

    //called when there is no empty cell left
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
  
        SpawnFood(new List<Vector2Int>());
    }

    public void ResetSpawner()
    {
        IsGridFull = false;
        SpawnFood(new List<Vector2Int>());
    }


    public void SpawnFood(List<Vector2Int> occupiedCells)
    {
        int totalCells = GridSystem.Instance.width * GridSystem.Instance.height;

        
        if (occupiedCells.Count >= totalCells)
        {
            IsGridFull = true;

           
            if (currentFoodObject != null)
            {
                currentFoodObject.SetActive(false);
            }

            OnGridFull?.Invoke();
            return;
        }

        
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