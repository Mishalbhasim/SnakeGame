using UnityEngine;


[RequireComponent(typeof(Camera))]
public class CameraFit : MonoBehaviour
{
    [Tooltip("Extra world-space units of empty space to leave on each side of the grid, so the border isn't flush against the screen edge.")]
    public float horizontalPadding = 1f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        FitToGrid();
    }

    public void FitToGrid()
    {
        if (GridSystem.Instance == null) return;

        float gridWorldWidth = GridSystem.Instance.width * GridSystem.Instance.cellSize;
        float gridWorldHeight = GridSystem.Instance.height * GridSystem.Instance.cellSize;

     
        float centerX = gridWorldWidth / 2f;
        float centerY = gridWorldHeight / 2f;
        transform.position = new Vector3(centerX, centerY, -10f);


        float requiredHalfWidth = (gridWorldWidth / 2f) + horizontalPadding;
        float sizeForWidth = requiredHalfWidth / cam.aspect;

        cam.orthographicSize = sizeForWidth;
    }
}