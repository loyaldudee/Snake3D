using UnityEngine;
using System.Collections.Generic;

public class FoodManager : MonoBehaviour
{
    public CubeGridGenerator grid;
    public SnakeSurfaceMover snake; 
    public GameObject foodPrefab; 

    [Range(0.1f, 2.0f)]
    public float foodScale = 1.0f; 

    public Vector3Int currentFoodGridPos;
    private GameObject currentFoodObject;

    void Start()
    {
        if (grid == null) grid = FindObjectOfType<CubeGridGenerator>();
    }

    public void SpawnFood()
    {
        if (snake == null) return;

        if (currentFoodObject != null) Destroy(currentFoodObject);

        Vector3Int spawnPos = Vector3Int.zero;
        bool validPositionFound = false;
        int attempts = 0;

        while (!validPositionFound && attempts < 100)
        {
            spawnPos = GetRandomSurfaceGridPos();
            if (!IsPositionOnSnake(spawnPos))
            {
                validPositionFound = true;
            }
            attempts++;
        }

        if (validPositionFound)
        {
            currentFoodGridPos = spawnPos;
            SpawnFoodVisuals(spawnPos);
        }
    }

    Vector3Int GetRandomSurfaceGridPos()
    {
        if (grid == null) return Vector3Int.zero;

        int n = grid.gridSize - 1;
        int side = Random.Range(0, 6);
        int u = Random.Range(0, grid.gridSize);
        int v = Random.Range(0, grid.gridSize);

        switch (side)
        {
            case 0: return new Vector3Int(u, v, 0); 
            case 1: return new Vector3Int(u, v, n); 
            case 2: return new Vector3Int(0, u, v); 
            case 3: return new Vector3Int(n, u, v); 
            case 4: return new Vector3Int(u, 0, v); 
            case 5: return new Vector3Int(u, n, v); 
        }
        return Vector3Int.zero;
    }

    bool IsPositionOnSnake(Vector3Int pos)
    {
        if (snake == null) return false;
        if (snake.gridPos == pos) return true; 
        return snake.IsBodyAt(pos); 
    }

    void SpawnFoodVisuals(Vector3Int gridPos)
    {
        float effectiveSpacing = grid.spacing;
        if (effectiveSpacing <= 0.001f)
        {
            float defaultGap = grid.gap > 0 ? grid.gap : 0.25f;
            effectiveSpacing = 1.0f + defaultGap; 
        }

        float n = grid.gridSize - 1;
        Vector3 offset = new Vector3((n * effectiveSpacing) / 2, (n * effectiveSpacing) / 2, (n * effectiveSpacing) / 2);
        
        Vector3 localPos = new Vector3(gridPos.x * effectiveSpacing, gridPos.y * effectiveSpacing, gridPos.z * effectiveSpacing) - offset;

        Vector3 normal = Vector3.zero;
        if (gridPos.x == 0) normal = Vector3.left;
        else if (gridPos.x == n) normal = Vector3.right;
        else if (gridPos.y == 0) normal = Vector3.down;
        else if (gridPos.y == n) normal = Vector3.up;
        else if (gridPos.z == 0) normal = Vector3.back;
        else if (gridPos.z == n) normal = Vector3.forward;

        float pushDistance = effectiveSpacing; 

        Vector3 spawnLocalPos = localPos + normal * pushDistance;

        currentFoodObject = Instantiate(foodPrefab, grid.transform);
        currentFoodObject.transform.localPosition = spawnLocalPos;
        currentFoodObject.transform.localRotation = Quaternion.LookRotation(normal);
        
        currentFoodObject.transform.localScale = Vector3.one * foodScale; 
    }
}