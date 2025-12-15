using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingTrap : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Trap Settings")]
    public float delayBeforeFall = 0.1f;
    public float detectionRange = 5f;
    public LayerMask playerLayer;

    private bool hasFallen = false;
    private bool hasActivated = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        Debug.Log("[FallingTrap] Initialized as Kinematic.");
    }

    private void Update()
    {
        if (!hasActivated)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, detectionRange, playerLayer);
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Debug.Log("[FallingTrap] Player detected below. Activating trap.");
                ActivateTrap();
            }
        }
    }

    public void ActivateTrap()
    {
        if (!hasFallen)
        {
            hasActivated = true;
            Debug.Log($"[FallingTrap] Activating trap. Falling in {delayBeforeFall}s...");
            Invoke(nameof(StartFalling), delayBeforeFall);
        }
    }

    private void StartFalling()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        hasFallen = true;
        Debug.Log("[FallingTrap] Trap is now falling.");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Không xử lý damage tại đây nữa. Giao lại cho spike hoặc surface bên dưới.
        Debug.Log("[FallingTrap] Collided with something.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * detectionRange);
    }
}
