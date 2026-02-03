using UnityEngine;

public class Chicken : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    
    private Rigidbody rb;
    private bool isRotating = false;
    private float rotationCooldown = 0.5f; // Prevent rapid rotation
    private float lastRotationTime = -1f;
    
    /// <summary>
    /// Z rotation we want to maintain so the chicken is always visible from
    /// a bird's‑eye / top‑down view.
    /// </summary>
    private const float TopDownZRotation = 90f;

    void Start()
    {
        // Get or add Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Ensure Rigidbody can rotate on Y axis (we still want to turn around on fences)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;

        // Top-down 2D style: rotate chicken so its full body is visible from above.
        // We only adjust visual orientation; movement still uses transform.forward.
        var euler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, TopDownZRotation);
        
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
        GameObject otherObject = collision.gameObject;

        // Fence: always turn immediately (no cooldown) so chickens never get stuck
        if (IsFence(otherObject))
        {
            Rotate180Degrees(ignoreCooldown: true);
            return;
        }

        // Chicken: use cooldown to prevent rapid flip-flop when two chickens meet
        if (Time.time - lastRotationTime < rotationCooldown)
            return;

        if (IsChicken(otherObject))
        {
            Rotate180Degrees();
            Chicken otherChicken = otherObject.GetComponent<Chicken>();
            if (otherChicken != null)
                otherChicken.Rotate180Degrees();
        }
    }
    
    /// <summary>
    /// Returns true if the object is (or is a child of) a fence, so we turn around.
    /// Checks tag "Fence" and walks up the hierarchy for names containing "Fence".
    /// </summary>
    private bool IsFence(GameObject obj)
    {
        if (obj == null) return false;
        if (obj.CompareTag("Fence")) return true;
        Transform t = obj.transform;
        while (t != null)
        {
            if (t.name.Contains("Fence")) return true;
            t = t.parent;
        }
        return false;
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
    
    /// <param name="ignoreCooldown">If true, rotate even during cooldown (used for fence so chickens don't get stuck).</param>
    public void Rotate180Degrees(bool ignoreCooldown = false)
    {
        if (!ignoreCooldown && Time.time - lastRotationTime < rotationCooldown)
            return;

        isRotating = true;
        lastRotationTime = Time.time;
        
        // Rotate the Rigidbody (not just transform) to ensure physics respects the rotation
        // but keep the Z rotation fixed so the chicken always stays at a top-down angle.
        float currentY = rb.rotation.eulerAngles.y;
        float newY = currentY + 180f;

        Quaternion newRot = Quaternion.Euler(0f, newY, TopDownZRotation);

        // Apply rotation to Rigidbody
        rb.rotation = newRot;

        // Also update transform to ensure consistency
        transform.rotation = newRot;
        
        // Stop current velocity to prevent immediate re-collision
        rb.linearVelocity = Vector3.zero;
    }
}
