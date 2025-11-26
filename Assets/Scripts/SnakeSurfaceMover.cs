using UnityEngine;
using System.Collections.Generic;

public class SnakeSurfaceMover : MonoBehaviour
{
    public CubeGridGenerator grid;
    
    [Header("Tail Settings")]
    public GameObject tailPrefab; // Drag your Cube prefab here
    public int initialTailLength = 3;

    [Header("Movement Settings")]
    public float stepInterval = 0.2f;
    public float raycastCheckDistance = 2.0f; 

    // --- TAIL DATA STRUCTURE ---
    struct TailSegment
    {
        public Vector3Int pos;
        public Vector3Int normal;

        public TailSegment(Vector3Int p, Vector3Int n)
        {
            pos = p;
            normal = n;
        }
    }

    private List<TailSegment> tailSegments = new List<TailSegment>();
    private List<GameObject> tailVisuals = new List<GameObject>();

    // State
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

        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null) snakeHalfSize = r.bounds.extents.x;
        else snakeHalfSize = 0.5f;

        // --- SPAWN FIX ---
        int mid = grid.gridSize / 2;
        int n = grid.gridSize - 1;
        
        // Spawn at Z = 0 (Closest to Camera) instead of Z = n
        gridPos = new Vector3Int(mid, mid, 0);
        localNormal = Vector3Int.back; // Facing the camera (-Z)

        SnapToSurface();

        // Initialize Tail
        for(int i = 0; i < initialTailLength; i++)
        {
            Grow();
        }
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
            tailVisuals.Add(obj);
        }
    }

    void HandleInput()
    {
        Vector3Int newDir = Vector3Int.zero;

        if (Input.GetKeyDown(KeyCode.W)) newDir = GetDirectionForInput(Vector3Int.up);
        if (Input.GetKeyDown(KeyCode.S)) newDir = GetDirectionForInput(Vector3Int.down);
        if (Input.GetKeyDown(KeyCode.A)) newDir = GetDirectionForInput(Vector3Int.left);
        if (Input.GetKeyDown(KeyCode.D)) newDir = GetDirectionForInput(Vector3Int.right);

        if (newDir != Vector3Int.zero)
        {
            if (newDir == localNormal || newDir == -localNormal) return;
            
            // Allow turning in any direction (Freedom of movement)
            dir = newDir;
            canMove = true;
        }
    }

    Vector3Int GetDirectionForInput(Vector3Int input)
    {
        // Front (+Z) or Back (-Z) Faces
        if (localNormal.z != 0) {
            if (input == Vector3Int.up) return Vector3Int.up;
            if (input == Vector3Int.down) return Vector3Int.down;
            
            // Adjust Left/Right for Back face so controls match camera view
            if (localNormal.z > 0) { // Front (Far side)
                if (input == Vector3Int.left) return Vector3Int.left; 
                if (input == Vector3Int.right) return Vector3Int.right; 
            }
            else { // Back (Camera side)
                // When looking at the back face, Left is +X (Right in grid space) and Right is -X
                if (input == Vector3Int.left) return Vector3Int.right; 
                if (input == Vector3Int.right) return Vector3Int.left; 
            }
        }
        // Top (+Y) or Bottom (-Y) Faces
        if (localNormal.y != 0) {
            if (localNormal.y > 0) { // Top
                if (input == Vector3Int.up) return Vector3Int.forward; 
                if (input == Vector3Int.down) return Vector3Int.back; 
            }
            else { // Bottom
                if (input == Vector3Int.up) return Vector3Int.back; 
                if (input == Vector3Int.down) return Vector3Int.forward; 
            }
            if (input == Vector3Int.left) return Vector3Int.left; 
            if (input == Vector3Int.right) return Vector3Int.right;
        }
        // Right (+X) or Left (-X) Faces
        if (localNormal.x != 0) {
            if (input == Vector3Int.up) return Vector3Int.up; 
            if (input == Vector3Int.down) return Vector3Int.down;
            if (localNormal.x > 0) { // Right
                if (input == Vector3Int.left) return Vector3Int.forward; 
                if (input == Vector3Int.right) return Vector3Int.back; 
            }
            else { // Left
                if (input == Vector3Int.left) return Vector3Int.back; 
                if (input == Vector3Int.right) return Vector3Int.forward; 
            }
        }
        return Vector3Int.zero;
    }

    void MoveStep()
    {
        Vector3Int nextPos = gridPos + dir;
        Vector3Int nextDir = dir;
        Vector3Int nextNormal = localNormal;
        
        int n = grid.gridSize - 1;

        // Wrap Logic
        bool outOfBounds = nextPos.x < 0 || nextPos.x > n || nextPos.y < 0 || nextPos.y > n || nextPos.z < 0 || nextPos.z > n;

        if (outOfBounds)
        {
            Wrap(ref nextPos, ref nextDir, ref nextNormal, n);
        }
        else if (!IsSurface(nextPos))
        {
            return;
        }

        // Tail Collision Check
        for(int i = 0; i < tailSegments.Count; i++)
        {
            if (tailSegments[i].pos == nextPos)
            {
                Debug.Log("GAME OVER - Hit Tail");
                canMove = false;
                return;
            }
        }

        // Update Tail History
        if (tailSegments.Count > 0)
        {
            tailSegments.Insert(0, new TailSegment(gridPos, localNormal));
            tailSegments.RemoveAt(tailSegments.Count - 1);
            UpdateTailVisuals();
        }

        // Move Head
        gridPos = nextPos;
        dir = nextDir;
        localNormal = nextNormal;
        lastDir = dir;

        SnapToSurface();
    }

    void UpdateTailVisuals()
    {
        for (int i = 0; i < tailVisuals.Count; i++)
        {
            if (i < tailSegments.Count)
            {
                TailSegment seg = tailSegments[i];
                GameObject vis = tailVisuals[i];

                Vector3 worldPos = GridToWorld(seg.pos);
                Vector3 worldNormal = grid.transform.TransformDirection((Vector3)seg.normal);
                
                float cubeExtent = grid.cubePrefab.GetComponentInChildren<Renderer>().bounds.extents.x;
                float desiredGap = 0.1f;
                float totalOffset = cubeExtent + desiredGap + snakeHalfSize;

                vis.transform.position = worldPos + worldNormal * totalOffset;
                vis.transform.rotation = Quaternion.LookRotation(grid.transform.up, worldNormal);
            }
        }
    }

    void Wrap(ref Vector3Int pos, ref Vector3Int direction, ref Vector3Int normal, int n)
    {
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

    bool IsSurface(Vector3Int g)
    {
        int n = grid.gridSize - 1;
        if (g.x < 0 || g.y < 0 || g.z < 0) return false;
        if (g.x > n || g.y > n || g.z > n) return false;
        return g.x == 0 || g.y == 0 || g.z == 0 || g.x == n || g.y == n || g.z == n;
    }

    Vector3 GridToWorld(Vector3Int g)
    {
        float n = grid.gridSize - 1;
        Vector3 offset = new Vector3((n * grid.spacing) / 2, (n * grid.spacing) / 2, (n * grid.spacing) / 2);
        Vector3 localPos = new Vector3(g.x * grid.spacing, g.y * grid.spacing, g.z * grid.spacing) - offset;
        return grid.transform.TransformPoint(localPos);
    }

    void SnapToSurface()
    {
        Vector3 theoreticalPos = GridToWorld(gridPos);
        Vector3 worldNormal = grid.transform.TransformDirection((Vector3)localNormal);
        Vector3 rayOrigin = theoreticalPos + worldNormal * grid.spacing * 1.5f;
        Ray ray = new Ray(rayOrigin, -worldNormal);
        RaycastHit hit;
        float visualOffset = 0.05f + snakeHalfSize;

        if (Physics.Raycast(ray, out hit, raycastCheckDistance))
        {
            if (hit.collider.CompareTag("grid"))
            {
                transform.position = hit.point + hit.normal * visualOffset;
                if (dir != Vector3Int.zero)
                {
                    Vector3 worldMoveDir = grid.transform.TransformDirection((Vector3)dir);
                    transform.rotation = Quaternion.LookRotation(worldMoveDir, hit.normal);
                }
                else transform.rotation = Quaternion.LookRotation(grid.transform.up, hit.normal);
                return;
            }
        }
        transform.position = theoreticalPos + worldNormal * (0.5f + visualOffset);
        if (dir != Vector3Int.zero)
        {
            Vector3 worldMoveDir = grid.transform.TransformDirection((Vector3)dir);
            transform.rotation = Quaternion.LookRotation(worldMoveDir, worldNormal);
        }
    }
}