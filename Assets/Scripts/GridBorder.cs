using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class GridBorder : MonoBehaviour
{
    [Tooltip("Thickness of the border line in world units")]
    public float lineWidth = 0.1f;

    [Tooltip("Color of the border")]
    public Color borderColor = Color.white;

    private void Start()
    {
        DrawBorder();
    }

    private void DrawBorder()
    {
        GridSystem grid = GetComponent<GridSystem>();
        if (grid == null)
        {
            Debug.LogError("GridBorder requires a GridSystem component on the same GameObject.");
            return;
        }

        LineRenderer lr = GetComponent<LineRenderer>();


        lr.positionCount = 5;
        lr.loop = false;
        lr.useWorldSpace = true;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

  
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = borderColor;
        lr.endColor = borderColor;

        float w = grid.width * grid.cellSize;
        float h = grid.height * grid.cellSize;

        Vector3 bottomLeft = new Vector3(0, 0, 0);
        Vector3 bottomRight = new Vector3(w, 0, 0);
        Vector3 topRight = new Vector3(w, h, 0);
        Vector3 topLeft = new Vector3(0, h, 0);

        lr.SetPosition(0, bottomLeft);
        lr.SetPosition(1, bottomRight);
        lr.SetPosition(2, topRight);
        lr.SetPosition(3, topLeft);
        lr.SetPosition(4, bottomLeft);


        lr.sortingOrder = -1;
    }
}