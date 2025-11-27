using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject snakeHeadPrefab;
    private GameObject snakeHead;

    void Start()
    {
        // 1. Setup Fixed Camera
        if (Camera.main != null)
        {
            // Position camera back along Z axis
            // Adjust -25f if the grid is too big or small on screen
            Camera.main.transform.position = new Vector3(0, 0, -25f); 
            Camera.main.transform.rotation = Quaternion.identity; // Look forward (0,0,0)
            
            // Remove old camera script if it exists to stop it from moving
            var oldCam = Camera.main.GetComponent<SnakeCamera>();
            if (oldCam != null) Destroy(oldCam);
        }

        SpawnSnake();
    }

    void SpawnSnake()
    {
        snakeHead = Instantiate(snakeHeadPrefab);

        SnakeSurfaceMover mover = snakeHead.GetComponent<SnakeSurfaceMover>();
        CubeGridGenerator gridGen = FindObjectOfType<CubeGridGenerator>();
        
        if (mover != null && gridGen != null)
        {
            mover.grid = gridGen;
            
            // Ensure Grid has the Rotator script
            GridRotator rotator = gridGen.GetComponent<GridRotator>();
            if (rotator == null) rotator = gridGen.gameObject.AddComponent<GridRotator>();
            
            mover.rotator = rotator;
        }
    }
}