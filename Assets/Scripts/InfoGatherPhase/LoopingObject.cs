using UnityEngine;

public class LoopingObject : MonoBehaviour
{
    // Speed of movement (editable in Inspector)
    public float speed = 5.0f;
    
    // The specific Z coordinates
    public float limitZ = 0.0f;
    public float resetZ = -25.0f;

    void Update()
    {
        // 1. Move the object forward along the global Z axis
        // We use Time.deltaTime to make movement smooth and frame-rate independent
        transform.position += Vector3.forward * speed * Time.deltaTime;

        // 2. Check if we have hit or passed the limit (Z = 0)
        if (transform.position.z <= limitZ)
        {
            // 3. Teleport back to Z = -25, keeping X and Y the same
            Vector3 newPosition = transform.position;
            newPosition.z = resetZ;
            transform.position = newPosition;
        }
    }
}