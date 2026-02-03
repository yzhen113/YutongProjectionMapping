using UnityEngine;

public class Chicken : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    
    private Rigidbody rb;
    private bool isRotating = false;
    private float rotationCooldown = 0.5f; // Prevent rapid rotation
    private float lastRotationTime = -1f;
    
    void Start()
    {
        // Get or add Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Ensure Rigidbody can rotate on Y axis
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        
        // Ensure the chicken has a collider for collision detection
        if (GetComponent<Collider>() == null)
        {
            // Try to get collider from children, or add a basic one
            Collider col = GetComponentInChildren<Collider>();
            if (col == null)
            {
                CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
                capsule.height = 1f;
                capsule.radius = 0.3f;
            }
        }
    }

    void FixedUpdate()
    {
        // Only move if not currently rotating (to prevent immediate re-collision)
        if (!isRotating)
        {
            // Move forward automatically
            Vector3 forwardMovement = transform.forward * moveSpeed;
            rb.linearVelocity = new Vector3(forwardMovement.x, rb.linearVelocity.y, forwardMovement.z);
        }
        
        // Reset rotation flag after cooldown
        if (isRotating && Time.time - lastRotationTime > rotationCooldown)
        {
            isRotating = false;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if enough time has passed since last rotation
        if (Time.time - lastRotationTime < rotationCooldown)
        {
            return;
        }
        
        GameObject otherObject = collision.gameObject;
        
        // First check if collision is with another chicken
        if (IsChicken(otherObject))
        {
            // Both chickens turn 180 degrees
            Rotate180Degrees();
            
            // Also make the other chicken turn 180 degrees
            Chicken otherChicken = otherObject.GetComponent<Chicken>();
            if (otherChicken != null)
            {
                otherChicken.Rotate180Degrees();
            }
            return;
        }
        
        // Check if the collision is with a fence (check both object name and parent)
        GameObject fenceObject = collision.gameObject;
        bool isFence = false;
        
        // Check the collided object's name
        if (fenceObject.name.Contains("Fence"))
        {
            isFence = true;
        }
        // Also check parent object name (fences might be children)
        else if (fenceObject.transform.parent != null && fenceObject.transform.parent.name.Contains("Fence"))
        {
            isFence = true;
        }
        
        if (isFence)
        {
            // Turn 180 degrees away from the fence
            Rotate180Degrees();
        }
    }
    
    private bool IsChicken(GameObject obj)
    {
        // Check if the object has a Chicken component
        Chicken otherChicken = obj.GetComponent<Chicken>();
        if (otherChicken != null && otherChicken != this)
        {
            return true;
        }
        
        // Also check by name as fallback
        string objName = obj.name.ToLower();
        if (objName.Contains("chicken"))
        {
            // Make sure it's not this chicken itself
            if (obj != gameObject)
            {
                return true;
            }
        }
        
        return false;
    }
    
    public void Rotate180Degrees()
    {
        // Check if enough time has passed since last rotation
        if (Time.time - lastRotationTime < rotationCooldown)
        {
            return;
        }
        
        isRotating = true;
        lastRotationTime = Time.time;
        
        // Rotate the Rigidbody (not just transform) to ensure physics respects the rotation
        float currentY = rb.rotation.eulerAngles.y;
        float newY = currentY + 180f;
        
        // Apply rotation to Rigidbody
        rb.rotation = Quaternion.Euler(0, newY, 0);
        
        // Also update transform to ensure consistency
        transform.rotation = Quaternion.Euler(0, newY, 0);
        
        // Stop current velocity to prevent immediate re-collision
        rb.linearVelocity = Vector3.zero;
    }
}