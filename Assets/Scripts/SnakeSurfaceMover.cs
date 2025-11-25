using UnityEngine;
using System.Collections.Generic;

public class SnakeSurfaceMover : MonoBehaviour
{
    public CubeGridGenerator grid;
    
    [Header("Settings")]
    public float stepInterval = 0.2f;
    public float raycastCheckDistance = 2.0f; 

    [Header("Debug Info")]
    public Vector3Int gridPos;
    public Vector3Int localNormal; // Tracks which face we are on
    
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

        // Initialize on Front Face Center (z = max)
        int mid = grid.gridSize / 2;
        int n = grid.gridSize - 1;
        
        gridPos = new Vector3Int(mid, mid, n);
        localNormal = Vector3Int.forward; // Start on Front Face

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
        Vector3Int newDir = Vector3Int.zero;

        // Determine input based on WHICH FACE we are on (Context Aware)
        if (Input.GetKeyDown(KeyCode.W)) newDir = GetDirectionForInput(Vector3Int.up);
        if (Input.GetKeyDown(KeyCode.S)) newDir = GetDirectionForInput(Vector3Int.down);
        if (Input.GetKeyDown(KeyCode.A)) newDir = GetDirectionForInput(Vector3Int.left);
        if (Input.GetKeyDown(KeyCode.D)) newDir = GetDirectionForInput(Vector3Int.right);

        // If we found a valid direction
        if (newDir != Vector3Int.zero)
        {
            // SAFETY CHECK: Prevent moving directly into/away from the surface
            // This fixes the "Shooting Off" bug
            if (newDir == localNormal || newDir == -localNormal) 
                return;

            // Prevent 180 degree turns
            if (newDir != -lastDir)
            {
                dir = newDir;
                canMove = true;
            }
        }
    }

    // Maps logical "Up/Down/Left/Right" to actual Grid Directions based on the Face
    Vector3Int GetDirectionForInput(Vector3Int input)
    {
        // Front Face (+Z) or Back Face (-Z) -> Standard Controls
        if (localNormal.z != 0)
        {
            if (input == Vector3Int.up) return Vector3Int.up;
            if (input == Vector3Int.down) return Vector3Int.down;
            
            // Adjust Left/Right for Back face so controls aren't inverted
            if (localNormal.z > 0) // Front
            {
                if (input == Vector3Int.left) return Vector3Int.left;
                if (input == Vector3Int.right) return Vector3Int.right;
            }
            else // Back
            {
                if (input == Vector3Int.left) return Vector3Int.right;
                if (input == Vector3Int.right) return Vector3Int.left;
            }
        }

        // Top Face (+Y) or Bottom Face (-Y) -> Y-axis moves map to Z-axis
        if (localNormal.y != 0)
        {
            if (localNormal.y > 0) // Top
            {
                if (input == Vector3Int.up) return Vector3Int.forward; // W moves +Z
                if (input == Vector3Int.down) return Vector3Int.back;  // S moves -Z
            }
            else // Bottom
            {
                if (input == Vector3Int.up) return Vector3Int.back;    // W moves -Z (away from cam)
                if (input == Vector3Int.down) return Vector3Int.forward;
            }

            if (input == Vector3Int.left) return Vector3Int.left;
            if (input == Vector3Int.right) return Vector3Int.right;
        }

        // Right Face (+X) or Left Face (-X) -> X-axis moves map to Z-axis
        if (localNormal.x != 0)
        {
            if (input == Vector3Int.up) return Vector3Int.up;
            if (input == Vector3Int.down) return Vector3Int.down;

            if (localNormal.x > 0) // Right
            {
                if (input == Vector3Int.left) return Vector3Int.forward; // A moves +Z
                if (input == Vector3Int.right) return Vector3Int.back;   // D moves -Z
            }
            else // Left
            {
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

        // Check Bounds
        bool outOfBounds = nextPos.x < 0 || nextPos.x > n || 
                           nextPos.y < 0 || nextPos.y > n || 
                           nextPos.z < 0 || nextPos.z > n;

        if (outOfBounds)
        {
            Wrap(ref nextPos, ref nextDir, ref nextNormal, n);
        }
        else if (!IsSurface(nextPos))
        {
            // Trying to burrow inside -> Stop
            return;
        }

        gridPos = nextPos;
        dir = nextDir;
        localNormal = nextNormal;
        lastDir = dir;

        SnapToSurface();
    }

    void Wrap(ref Vector3Int pos, ref Vector3Int direction, ref Vector3Int normal, int n)
    {
        // 1. TOP FACE (UP)
        if (normal == Vector3Int.up)
        {
            if (pos.x > n) { pos = new Vector3Int(n, n-1, pos.z); direction = Vector3Int.down; normal = Vector3Int.right; }
            else if (pos.x < 0) { pos = new Vector3Int(0, n-1, pos.z); direction = Vector3Int.down; normal = Vector3Int.left; }
            else if (pos.z > n) { pos = new Vector3Int(pos.x, n-1, n); direction = Vector3Int.down; normal = Vector3Int.forward; }
            else if (pos.z < 0) { pos = new Vector3Int(pos.x, n-1, 0); direction = Vector3Int.down; normal = Vector3Int.back; }
        }
        // 2. BOTTOM FACE (DOWN)
        else if (normal == Vector3Int.down)
        {
            if (pos.x > n) { pos = new Vector3Int(n, 1, pos.z); direction = Vector3Int.up; normal = Vector3Int.right; }
            else if (pos.x < 0) { pos = new Vector3Int(0, 1, pos.z); direction = Vector3Int.up; normal = Vector3Int.left; }
            else if (pos.z > n) { pos = new Vector3Int(pos.x, 1, n); direction = Vector3Int.up; normal = Vector3Int.forward; }
            else if (pos.z < 0) { pos = new Vector3Int(pos.x, 1, 0); direction = Vector3Int.up; normal = Vector3Int.back; }
        }
        // 3. RIGHT FACE (RIGHT)
        else if (normal == Vector3Int.right)
        {
            if (pos.y > n) { pos = new Vector3Int(n-1, n, pos.z); direction = Vector3Int.left; normal = Vector3Int.up; }
            else if (pos.y < 0) { pos = new Vector3Int(n-1, 0, pos.z); direction = Vector3Int.left; normal = Vector3Int.down; }
            else if (pos.z > n) { pos = new Vector3Int(n-1, pos.y, n); direction = Vector3Int.left; normal = Vector3Int.forward; }
            else if (pos.z < 0) { pos = new Vector3Int(n-1, pos.y, 0); direction = Vector3Int.left; normal = Vector3Int.back; }
        }
        // 4. LEFT FACE (LEFT)
        else if (normal == Vector3Int.left)
        {
            if (pos.y > n) { pos = new Vector3Int(1, n, pos.z); direction = Vector3Int.right; normal = Vector3Int.up; }
            else if (pos.y < 0) { pos = new Vector3Int(1, 0, pos.z); direction = Vector3Int.right; normal = Vector3Int.down; }
            else if (pos.z > n) { pos = new Vector3Int(1, pos.y, n); direction = Vector3Int.right; normal = Vector3Int.forward; }
            else if (pos.z < 0) { pos = new Vector3Int(1, pos.y, 0); direction = Vector3Int.right; normal = Vector3Int.back; }
        }
        // 5. FRONT FACE (FORWARD)
        else if (normal == Vector3Int.forward)
        {
            if (pos.y > n) { pos = new Vector3Int(pos.x, n, n-1); direction = Vector3Int.back; normal = Vector3Int.up; }
            else if (pos.y < 0) { pos = new Vector3Int(pos.x, 0, n-1); direction = Vector3Int.back; normal = Vector3Int.down; }
            else if (pos.x > n) { pos = new Vector3Int(n, pos.y, n-1); direction = Vector3Int.back; normal = Vector3Int.right; }
            else if (pos.x < 0) { pos = new Vector3Int(0, pos.y, n-1); direction = Vector3Int.back; normal = Vector3Int.left; }
        }
        // 6. BACK FACE (BACK)
        else if (normal == Vector3Int.back)
        {
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

        // Raycast Check
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
                else
                {
                    transform.rotation = Quaternion.LookRotation(grid.transform.up, hit.normal);
                }
                return;
            }
        }

        // Fallback if Raycast fails
        transform.position = theoreticalPos + worldNormal * (0.5f + visualOffset);
        if (dir != Vector3Int.zero)
        {
            Vector3 worldMoveDir = grid.transform.TransformDirection((Vector3)dir);
            transform.rotation = Quaternion.LookRotation(worldMoveDir, worldNormal);
        }
    }
}