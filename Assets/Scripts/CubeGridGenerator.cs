using System.Collections.Generic;
using UnityEngine;

public class CubeGridGenerator : MonoBehaviour
{
    public GameObject cubePrefab;
    public int gridSize = 5;
    public float gap = 0.25f;
    public float cubeSize;

    public List<Vector3> allCubePositions = new List<Vector3>();
    public List<Vector3> surfacePositions = new List<Vector3>();
    public List<Vector3> frontFacePositions = new List<Vector3>();

    public Dictionary<Vector3, Vector3Int> worldToGrid = new Dictionary<Vector3, Vector3Int>();

    public float spacing;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        Renderer r = cubePrefab.GetComponentInChildren<Renderer>();
        float cubeSize = r.bounds.size.x;
        spacing = cubeSize + gap;

        float totalSize = (gridSize - 1) * spacing;
        Vector3 centerOffset = new Vector3(totalSize/2, totalSize/2, totalSize/2);

        for (int x=0; x<gridSize; x++)
        {
            for (int y=0; y<gridSize; y++)
            {
                for (int z=0; z<gridSize; z++)
                {
                    Vector3 pos = new Vector3(x*spacing, y*spacing, z*spacing) - centerOffset;
                    Vector3 worldPos = transform.TransformPoint(pos);

                    Instantiate(cubePrefab, pos, Quaternion.identity, transform);

                    allCubePositions.Add(worldPos);
                    worldToGrid[worldPos] = new Vector3Int(x,y,z);

                    bool isSurface =
                        x == 0 || y == 0 || z == 0 ||
                        x == gridSize-1 || y == gridSize-1 || z == gridSize-1;

                    if (isSurface)
                        surfacePositions.Add(worldPos);

                    // front face = z == max
                    if (z == gridSize-1)
                        frontFacePositions.Add(worldPos);
                }
            }
        }

        Debug.Log("Total cubes: " + allCubePositions.Count);
        Debug.Log("Surface cubes: " + surfacePositions.Count);
        Debug.Log("Front face cubes: " + frontFacePositions.Count);
    }

}
