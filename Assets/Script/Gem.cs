using UnityEngine;

public class GemProximityGlow : MonoBehaviour
{
    [Header("Proximity Settings")]
    public float glowDistance = 1.5f;
    public Color glowColor = Color.yellow;

    private Transform player;
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        // Find the player once
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    void Update()
    {
        if (player == null || rend == null) return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= glowDistance)
        {
            rend.material.color = glowColor;
        }
        else
        {
            rend.material.color = originalColor;
        }
    }
}
