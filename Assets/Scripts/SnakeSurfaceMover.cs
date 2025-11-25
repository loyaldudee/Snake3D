using UnityEngine;

public class SnakeSurfaceMover : MonoBehaviour
{
    public CubeGridGenerator grid;

    public Vector3Int gridPos;
    public float stepInterval = 0.2f;
    float timer;

    Vector3Int dir = Vector3Int.zero;
    bool canMove = false;

    Renderer snakeRenderer;
    float snakeExtent;   // half of snake size

    void Start()
    {
        if (grid == null)
            grid = FindObjectOfType<CubeGridGenerator>();

        snakeRenderer = GetComponentInChildren<Renderer>();
        snakeExtent = snakeRenderer.bounds.extents.x;   // half width of snake

        int mid = grid.gridSize / 2;
        int n = grid.gridSize - 1;

        gridPos = new Vector3Int(mid, mid, n);

        SnapToSurface();
    }

    void Update()
    {
        HandleInput();

        if (!canMove) return;

        timer += Time.deltaTime;
        if (timer >= stepInterval)
        {
            timer = 0;
            MoveStep();
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) dir = Vector3Int.up;
        if (Input.GetKeyDown(KeyCode.S)) dir = Vector3Int.down;
        if (Input.GetKeyDown(KeyCode.A)) dir = Vector3Int.left;
        if (Input.GetKeyDown(KeyCode.D)) dir = Vector3Int.right;

        if (dir != Vector3Int.zero)
            canMove = true;
    }

    void MoveStep()
    {
        Vector3Int next = gridPos + dir;

        if (!IsSurface(next))
            return;

        gridPos = next;
        SnapToSurface();
    }

    bool IsSurface(Vector3Int g)
    {
        int n = grid.gridSize - 1;

        if (g.x < 0 || g.y < 0 || g.z < 0) return false;
        if (g.x > n || g.y > n || g.z > n) return false;

        return g.x == 0 || g.y == 0 || g.z == 0 ||
               g.x == n || g.y == n || g.z == n;
    }

    Vector3 GridToWorld(Vector3Int g)
    {
        float n = grid.gridSize - 1;

        Vector3 offset = new Vector3(
            (n * grid.spacing) / 2,
            (n * grid.spacing) / 2,
            (n * grid.spacing) / 2
        );

        Vector3 localPos = new Vector3(
            g.x * grid.spacing,
            g.y * grid.spacing,
            g.z * grid.spacing
        ) - offset;

        return grid.transform.TransformPoint(localPos);
    }

    Vector3 GetNormalForGridPos(Vector3Int g)
    {
        int n = grid.gridSize - 1;

        if (g.z == n) return Vector3.forward;
        if (g.z == 0) return Vector3.back;
        if (g.x == n) return Vector3.right;
        if (g.x == 0) return Vector3.left;
        if (g.y == n) return Vector3.up;
        if (g.y == 0) return Vector3.down;

        return Vector3.forward;
    }

    void SnapToSurface()
    {
        Vector3 basePos = GridToWorld(gridPos);
        Vector3 normal = GetNormalForGridPos(gridPos);

        // cube surface distance (actual mesh extent)
        float cubeExtent = grid.cubePrefab.GetComponentInChildren<Renderer>().bounds.extents.x;

        float desiredGap = 0.1f;

        float offset = cubeExtent + desiredGap + snakeExtent;

        transform.position = basePos + normal * offset;
    }
}
