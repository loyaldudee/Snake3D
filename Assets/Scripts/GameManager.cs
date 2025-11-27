using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject snakeHeadPrefab;
    private GameObject snakeHead;

    void Start()
    {
        // 1. Setup Fixed Camera Position
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0, 0, -18f); 
            Camera.main.transform.rotation = Quaternion.identity;
            
            var oldCam = Camera.main.GetComponent<SnakeCamera>();
            if (oldCam != null) Destroy(oldCam);
        }

        SpawnSnake();
    }

    void SpawnSnake()
    {
        // 1. Spawn the Snake Instance
        snakeHead = Instantiate(snakeHeadPrefab);
        SnakeSurfaceMover snakeMover = snakeHead.GetComponent<SnakeSurfaceMover>();

        // 2. Find Key Objects in the Scene
        CubeGridGenerator gridGen = FindObjectOfType<CubeGridGenerator>();
        FoodManager foodManager = FindObjectOfType<FoodManager>();
        
        // 3. Connect Grid to Snake
        if (snakeMover != null && gridGen != null)
        {
            snakeMover.grid = gridGen;
            
            // Ensure Rotator exists on the Grid object
            GridRotator rotator = gridGen.GetComponent<GridRotator>();
            if (rotator == null) rotator = gridGen.gameObject.AddComponent<GridRotator>();
            
            snakeMover.rotator = rotator;
        }

        // 4. Connect Everything to Food Manager (CRITICAL FIX)
        if (foodManager != null)
        {
            // Fix 1: Assign the Grid manually so FoodManager doesn't have to wait for Start()
            if (gridGen != null)
            {
                foodManager.grid = gridGen;
            }

            // Fix 2: Assign the Snake
            if (snakeMover != null)
            {
                foodManager.snake = snakeMover;
                snakeMover.foodManager = foodManager; // Connect back
                
                // Now it's safe to spawn
                foodManager.SpawnFood();
            }
        }
        else
        {
            Debug.LogError("Could not find FoodManager in the scene!");
        }
    }
}