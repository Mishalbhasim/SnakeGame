using UnityEngine;


public class GridSystem : MonoBehaviour
{
    [Header("Grid Dimensions")]
    [Tooltip("Number of cells horizontally")]
    public int width = 20;

    [Tooltip("Number of cells vertically")]
    public int height = 20;

    [Tooltip("World-space size of one cell (keep at 1 for simplicity)")]
    public float cellSize = 1f;

    public static GridSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * cellSize + cellSize * 0.5f,
            gridPos.y * cellSize + cellSize * 0.5f,
            0f
        );
    }


    public bool IsInsideGrid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < width
            && gridPos.y >= 0 && gridPos.y < height;
    }

    public Vector2Int GetRandomCell()
    {
        int x = Random.Range(0, width);
        int y = Random.Range(0, height);
        return new Vector2Int(x, y);
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 bottomLeft = new Vector3(0, 0, 0);
        Vector3 bottomRight = new Vector3(width * cellSize, 0, 0);
        Vector3 topLeft = new Vector3(0, height * cellSize, 0);
        Vector3 topRight = new Vector3(width * cellSize, height * cellSize, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}