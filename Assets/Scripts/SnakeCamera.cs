using UnityEngine;

public class SnakeCamera : MonoBehaviour
{
    public Transform snakeHead;
    private SnakeSurfaceMover snakeMover; 
    
    [Header("Settings")]
    public float distanceFromGrid = 20f; 
    public float positionSmoothTime = 0.2f;
    public float rotationSmoothSpeed = 5.0f;

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        if (snakeHead != null)
        {
            snakeMover = snakeHead.GetComponent<SnakeSurfaceMover>();
        }
    }

    void LateUpdate()
    {
        if (snakeHead == null) return;
        if (snakeMover == null) snakeMover = snakeHead.GetComponent<SnakeSurfaceMover>();
        if (snakeMover == null || snakeMover.grid == null) return;

        // 1. Calculate Target Position (Centered on Grid Face)
        Vector3 gridCenter = snakeMover.grid.transform.position;
        Vector3 faceNormal = snakeMover.grid.transform.TransformDirection((Vector3)snakeMover.localNormal);
        Vector3 targetPosition = gridCenter + (faceNormal * distanceFromGrid);

        // 2. Smooth Position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionSmoothTime);

        // 3. Calculate Rotation (PURE ORBIT STABILITY)
        // Look directly at the center of the grid
        Vector3 forwardVector = (gridCenter - transform.position).normalized;
        
        // Default Up is World Up (Y-axis)
        // This keeps the camera "upright" for Front, Back, Left, Right faces.
        Vector3 upReference = Vector3.up;

        // Handle the Poles (Top/Bottom faces) where World Up creates gimbal lock.
        // We check if our forward vector is nearly parallel to World Up.
        if (Mathf.Abs(Vector3.Dot(forwardVector, Vector3.up)) > 0.99f)
        {
            // If looking straight down/up, use Forward (Z-axis) as the temporary Up reference.
            // This effectively locks the rotation to a "top-down map" view without spinning.
            upReference = Vector3.forward;
        }

        // Use LookRotation. This creates a rotation looking at 'forwardVector' 
        // with the top of the camera pointing as close to 'upReference' as possible.
        Quaternion targetRotation = Quaternion.LookRotation(forwardVector, upReference);

        // 4. Smooth Rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
    }
}