using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject snakeHeadPrefab;

    private GameObject snakeHead;

    void Start()
    {
        SpawnSnake();
    }

    void SpawnSnake()
    {
        snakeHead = Instantiate(snakeHeadPrefab);

        SnakeSurfaceMover mover = snakeHead.GetComponent<SnakeSurfaceMover>();

        // Assign the grid generator using the NEW variable name
        mover.grid = FindObjectOfType<CubeGridGenerator>();
    }
}
