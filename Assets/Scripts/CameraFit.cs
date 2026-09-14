using UnityEngine;

/// <summary>
/// Automatically sizes and positions the orthographic camera so the entire grid
/// width is always visible, regardless of the device's screen aspect ratio.
/// This matters especially for Android, where phones vary a lot in aspect ratio
/// (9:16, 9:18, 9:19.5, 9:20, etc.) - a fixed camera size that looks right on
/// one phone can clip the grid on another.
/// Attach to the Main Camera.
/// </summary>
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

    /// <summary>
    /// Recalculates camera size/position based on the current GridSystem dimensions
    /// and the current screen aspect ratio. Called once at Start; call again manually
    /// if the grid size or screen orientation ever changes at runtime.
    /// </summary>
    public void FitToGrid()
    {
        if (GridSystem.Instance == null) return;

        float gridWorldWidth = GridSystem.Instance.width * GridSystem.Instance.cellSize;
        float gridWorldHeight = GridSystem.Instance.height * GridSystem.Instance.cellSize;

        // Center the camera on the middle of the grid
        float centerX = gridWorldWidth / 2f;
        float centerY = gridWorldHeight / 2f;
        transform.position = new Vector3(centerX, centerY, -10f);

        // Orthographic size is HALF the vertical view height.
        // camera.aspect = screen width / screen height.
        // Visible world width = orthographicSize * aspect * 2.
        // We solve for the orthographicSize needed so visible width covers the
        // grid width plus padding on both sides.
        float requiredHalfWidth = (gridWorldWidth / 2f) + horizontalPadding;
        float sizeForWidth = requiredHalfWidth / cam.aspect;

        cam.orthographicSize = sizeForWidth;
    }
}