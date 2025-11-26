using UnityEngine;

public class GridRotator : MonoBehaviour
{
    public Quaternion targetRotation = Quaternion.identity;
    public float smoothSpeed = 5.0f;

    public void SetTargetRotation(Quaternion rot)
    {
        targetRotation = rot;
    }

    void Update()
    {
        // Smoothly rotate towards the target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}