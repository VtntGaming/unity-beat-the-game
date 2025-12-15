using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class StandStillObject : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D solidCollider;
    private BoxCollider2D triggerCollider;

    [Header("Launch Settings")]
    [Tooltip("Tốc độ đẩy lên tối thiểu")]
    public float minLaunchForce = 2f;
    [Tooltip("Tốc độ đẩy lên tối đa")]
    public float maxLaunchForce = 5f;
    [Tooltip("Độ xê dịch ngang ngẫu nhiên")]
    public float horizontalNudge = 1f;

    [SerializeField] private LayerMask groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Assume two colliders: first is solid, second is trigger
        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
        if (colliders.Length < 2)
        {
            Debug.LogError("This object needs 2 BoxCollider2D components (one solid, one trigger)");
            return;
        }

        solidCollider = colliders[0];
        triggerCollider = colliders[1];

        solidCollider.isTrigger = false;
        triggerCollider.isTrigger = true;
        triggerCollider.enabled = true; // luôn bật trigger để có thể nhặt bất cứ lúc nào

        // Ignore collision với các object khác có cùng tag "Object"
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Object");
        foreach (var obj in allObjects)
        {
            if (obj != this.gameObject)
            {
                Collider2D otherSolid = obj.GetComponent<BoxCollider2D>();
                if (otherSolid != null)
                {
                    Physics2D.IgnoreCollision(solidCollider, otherSolid);
                }
            }
        }
    }

    void Start()
    {
        // random góc ngang
        float randomX = Random.Range(-horizontalNudge, horizontalNudge);
        // random lực đẩy lên
        float randomY = Random.Range(minLaunchForce, maxLaunchForce);

        rb.linearVelocity = new Vector2(randomX, randomY);
    }

    void FixedUpdate()
    {
        if (rb.bodyType == RigidbodyType2D.Static) return;

        if (IsGrounded() && rb.linearVelocity.magnitude < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;

            solidCollider.enabled = false;

            // 👉 Gọi FloatingMotion
            FloatingMotion floatMotion = GetComponent<FloatingMotion>();
            if (floatMotion != null)
            {
                floatMotion.ActivateFloating();
            }
        }
    }


    private bool IsGrounded()
    {
        float minY = solidCollider.bounds.min.y;
        Vector2 checkPos = new Vector2(solidCollider.bounds.center.x, minY - 0.15f);
        Vector2 checkSize = new Vector2(solidCollider.bounds.size.x * 0.95f, 0.25f);
        Collider2D hit = Physics2D.OverlapBox(checkPos, checkSize, 0f, groundLayer);
        return hit != null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (solidCollider == null) return;

        float minY = solidCollider.bounds.min.y;
        Vector2 groundCheckPos = new Vector2(solidCollider.bounds.center.x, minY - 0.15f);
        Vector2 groundCheckSize = new Vector2(solidCollider.bounds.size.x * 0.95f, 0.25f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheckPos, groundCheckSize);
    }
#endif
}
