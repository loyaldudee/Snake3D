using UnityEngine;
using System.Collections.Generic;

public class SnakeSurfaceMover : MonoBehaviour
{
    public CubeGridGenerator grid;
    public GridRotator rotator; 
    
    [Header("Tail Settings")]
    public GameObject tailPrefab; 
    public int initialTailLength = 3;

    [Header("Movement Settings")]
    public float stepInterval = 0.2f;
    public float raycastCheckDistance = 2.0f; 

    struct TailSegment
    {
        public Vector3Int pos;
        public Vector3Int normal;
        public TailSegment(Vector3Int p, Vector3Int n) { pos = p; normal = n; }
    }

    private List<TailSegment> tailSegments = new List<TailSegment>();
    private List<GameObject> tailVisuals = new List<GameObject>();

    public Vector3Int gridPos;
    public Vector3Int localNormal; 
    
    float timer;
    Vector3Int dir = Vector3Int.zero;
    Vector3Int lastDir = Vector3Int.zero;
    bool canMove = false;
    float snakeHalfSize;

    void Start()
    {
        if (grid == null) grid = FindObjectOfType<CubeGridGenerator>();
        if (rotator == null && grid != null) rotator = grid.GetComponent<GridRotator>();

        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null) snakeHalfSize = r.bounds.extents.x;
        else snakeHalfSize = 0.5f;

        // Start on Front Face (closest to camera Z=0)
        int mid = grid.gridSize / 2;
        int n = grid.gridSize - 1;
        
        gridPos = new Vector3Int(mid, mid, 0); 
        localNormal = Vector3Int.back; // Normal points to -Z (Camera)

        if (transform.parent != grid.transform) transform.SetParent(grid.transform);

        SnapToSurface();
        UpdateGridRotation(); 

        for(int i = 0; i < initialTailLength; i++) Grow();
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

    public void Grow()
    {
        Vector3Int spawnPos = gridPos;
        Vector3Int spawnNormal = localNormal;
        if (tailSegments.Count > 0)
        {
            spawnPos = tailSegments[tailSegments.Count - 1].pos;
            spawnNormal = tailSegments[tailSegments.Count - 1].normal;
        }
        tailSegments.Add(new TailSegment(spawnPos, spawnNormal));
        if (tailPrefab != null)
        {
            GameObject obj = Instantiate(tailPrefab, transform.position, Quaternion.identity);
            obj.transform.SetParent(grid.transform); 
            tailVisuals.Add(obj);
        }
    }

    void HandleInput()
    {
        if (Camera.main == null) return;

        Vector3Int newDir = Vector3Int.zero;
        
        // 1. Get Input Vector relative to Screen/Camera
        Vector3 inputVec = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.W)) inputVec = Camera.main.transform.up;
        if (Input.GetKeyDown(KeyCode.S)) inputVec = -Camera.main.transform.up;
        if (Input.GetKeyDown(KeyCode.A)) inputVec = -Camera.main.transform.right;
        if (Input.GetKeyDown(KeyCode.D)) inputVec = Camera.main.transform.right;

        if (inputVec == Vector3.zero) return;

        // 2. Transform this World Space input into Grid Local Space
        // Since the grid rotates, "Up" in world space might be "Left" in grid space.
        Vector3 localInput = grid.transform.InverseTransformDirection(inputVec);

        // 3. Snap to the closest Grid Axis (Up, Down, Left, Right, Forward, Back)
        // This automatically handles all rotation cases.
        newDir = Vector3Int.RoundToInt(localInput.normalized);

        // 4. Validation:
        // - Cannot move into/away from surface (Normal)
        // - Cannot reverse (180 turn)
        if (newDir != Vector3Int.zero)
        {
            // Check against local normal (current face)
            if (newDir == localNormal || newDir == -localNormal) return;

            // Standard Snake Rule: No 180 turns
            if (newDir != -lastDir) 
            {
                dir = newDir;
                canMove = true;
            }
        }
    }

    void MoveStep()
    {
        Vector3Int nextPos = gridPos + dir;
        Vector3Int nextDir = dir;
        Vector3Int nextNormal = localNormal;
        int n = grid.gridSize - 1;

        bool outOfBounds = nextPos.x < 0 || nextPos.x > n || nextPos.y < 0 || nextPos.y > n || nextPos.z < 0 || nextPos.z > n;

        if (outOfBounds) Wrap(ref nextPos, ref nextDir, ref nextNormal, n);
        else if (!IsSurface(nextPos)) return;

        for(int i = 0; i < tailSegments.Count; i++)
        {
            if (tailSegments[i].pos == nextPos) { Debug.Log("GAME OVER"); canMove = false; return; }
        }

        if (tailSegments.Count > 0)
        {
            tailSegments.Insert(0, new TailSegment(gridPos, localNormal));
            tailSegments.RemoveAt(tailSegments.Count - 1);
            UpdateTailVisuals();
        }

        gridPos = nextPos;
        dir = nextDir;
        localNormal = nextNormal;
        lastDir = dir;

        SnapToSurface();
        UpdateGridRotation(); 
    }

    void UpdateGridRotation()
    {
        if (rotator == null) return;

        // Target: Rotate grid so 'localNormal' points to Camera (World -Z)
        // AND 'dir' (movement) points to Camera Up (World +Y) if possible.
        
        Vector3 faceNormal = (Vector3)localNormal;
        Vector3 upReference = (dir != Vector3Int.zero) ? (Vector3)dir : Vector3.up;

        // We want the inverse of the rotation that aligns (Normal -> Back, Move -> Up)
        Quaternion targetWorldOrientation = Quaternion.LookRotation(Vector3.back, Vector3.up);
        Quaternion faceOrientation = Quaternion.LookRotation(faceNormal, upReference);
        
        Quaternion targetRot = targetWorldOrientation * Quaternion.Inverse(faceOrientation);
        
        rotator.SetTargetRotation(targetRot);
    }

    void UpdateTailVisuals()
    {
        for (int i = 0; i < tailVisuals.Count; i++)
        {
            if (i < tailSegments.Count)
            {
                TailSegment seg = tailSegments[i];
                GameObject vis = tailVisuals[i];
                Vector3 localPos = GetLocalGridPosition(seg.pos);
                Vector3 normal = (Vector3)seg.normal;
                float cubeExtent = grid.cubePrefab.GetComponentInChildren<Renderer>().bounds.extents.x;
                float totalOffset = cubeExtent + 0.1f + snakeHalfSize;
                vis.transform.localPosition = localPos + normal * totalOffset;
                vis.transform.localRotation = Quaternion.LookRotation(normal);
            }
        }
    }
    
    Vector3 GetLocalGridPosition(Vector3Int g)
    {
        float n = grid.gridSize - 1;
        Vector3 offset = new Vector3((n * grid.spacing) / 2, (n * grid.spacing) / 2, (n * grid.spacing) / 2);
        return new Vector3(g.x * grid.spacing, g.y * grid.spacing, g.z * grid.spacing) - offset;
    }

    void SnapToSurface()
    {
        if (transform.parent != grid.transform) transform.SetParent(grid.transform);

        Vector3 localPos = GetLocalGridPosition(gridPos);
        Vector3 normal = (Vector3)localNormal;
        
        float cubeExtent = grid.cubePrefab.GetComponentInChildren<Renderer>().bounds.extents.x;
        float offset = cubeExtent + 0.1f + snakeHalfSize;

        transform.localPosition = localPos + normal * offset;
        
        // ORIENTATION FIX: Rotate snake to face movement direction, with 'Up' as surface normal
        if (dir != Vector3Int.zero)
            transform.localRotation = Quaternion.LookRotation((Vector3)dir, normal);
        else
            transform.localRotation = Quaternion.LookRotation(Vector3.up, normal);
    }

    void Wrap(ref Vector3Int pos, ref Vector3Int direction, ref Vector3Int normal, int n) {
        if (normal == Vector3Int.up) {
            if (pos.x > n) { pos = new Vector3Int(n, n-1, pos.z); direction = Vector3Int.down; normal = Vector3Int.right; }
            else if (pos.x < 0) { pos = new Vector3Int(0, n-1, pos.z); direction = Vector3Int.down; normal = Vector3Int.left; }
            else if (pos.z > n) { pos = new Vector3Int(pos.x, n-1, n); direction = Vector3Int.down; normal = Vector3Int.forward; }
            else if (pos.z < 0) { pos = new Vector3Int(pos.x, n-1, 0); direction = Vector3Int.down; normal = Vector3Int.back; }
        }
        else if (normal == Vector3Int.down) {
            if (pos.x > n) { pos = new Vector3Int(n, 1, pos.z); direction = Vector3Int.up; normal = Vector3Int.right; }
            else if (pos.x < 0) { pos = new Vector3Int(0, 1, pos.z); direction = Vector3Int.up; normal = Vector3Int.left; }
            else if (pos.z > n) { pos = new Vector3Int(pos.x, 1, n); direction = Vector3Int.up; normal = Vector3Int.forward; }
            else if (pos.z < 0) { pos = new Vector3Int(pos.x, 1, 0); direction = Vector3Int.up; normal = Vector3Int.back; }
        }
        else if (normal == Vector3Int.right) {
            if (pos.y > n) { pos = new Vector3Int(n-1, n, pos.z); direction = Vector3Int.left; normal = Vector3Int.up; }
            else if (pos.y < 0) { pos = new Vector3Int(n-1, 0, pos.z); direction = Vector3Int.left; normal = Vector3Int.down; }
            else if (pos.z > n) { pos = new Vector3Int(n-1, pos.y, n); direction = Vector3Int.left; normal = Vector3Int.forward; }
            else if (pos.z < 0) { pos = new Vector3Int(n-1, pos.y, 0); direction = Vector3Int.left; normal = Vector3Int.back; }
        }
        else if (normal == Vector3Int.left) {
            if (pos.y > n) { pos = new Vector3Int(1, n, pos.z); direction = Vector3Int.right; normal = Vector3Int.up; }
            else if (pos.y < 0) { pos = new Vector3Int(1, 0, pos.z); direction = Vector3Int.right; normal = Vector3Int.down; }
            else if (pos.z > n) { pos = new Vector3Int(1, pos.y, n); direction = Vector3Int.right; normal = Vector3Int.forward; }
            else if (pos.z < 0) { pos = new Vector3Int(1, pos.y, 0); direction = Vector3Int.right; normal = Vector3Int.back; }
        }
        else if (normal == Vector3Int.forward) {
            if (pos.y > n) { pos = new Vector3Int(pos.x, n, n-1); direction = Vector3Int.back; normal = Vector3Int.up; }
            else if (pos.y < 0) { pos = new Vector3Int(pos.x, 0, n-1); direction = Vector3Int.back; normal = Vector3Int.down; }
            else if (pos.x > n) { pos = new Vector3Int(n, pos.y, n-1); direction = Vector3Int.back; normal = Vector3Int.right; }
            else if (pos.x < 0) { pos = new Vector3Int(0, pos.y, n-1); direction = Vector3Int.back; normal = Vector3Int.left; }
        }
        else if (normal == Vector3Int.back) {
            if (pos.y > n) { pos = new Vector3Int(pos.x, n, 1); direction = Vector3Int.forward; normal = Vector3Int.up; }
            else if (pos.y < 0) { pos = new Vector3Int(pos.x, 0, 1); direction = Vector3Int.forward; normal = Vector3Int.down; }
            else if (pos.x > n) { pos = new Vector3Int(n, pos.y, 1); direction = Vector3Int.forward; normal = Vector3Int.right; }
            else if (pos.x < 0) { pos = new Vector3Int(0, pos.y, 1); direction = Vector3Int.forward; normal = Vector3Int.left; }
        }
    }
    bool IsSurface(Vector3Int g) { int n = grid.gridSize - 1; if (g.x < 0 || g.y < 0 || g.z < 0) return false; if (g.x > n || g.y > n || g.z > n) return false; return g.x == 0 || g.y == 0 || g.z == 0 || g.x == n || g.y == n || g.z == n; }
}