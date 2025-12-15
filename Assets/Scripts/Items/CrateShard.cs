using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class CrateShard : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D solidCollider;

    [Header("Launch Settings")]
    [Tooltip("Độ xê dịch ngang ngẫu nhiên")]
    public float horizontalNudge = 1f;
    [Tooltip("Lực đẩy lên tối thiểu")]
    public float minLaunchForce = 2f;
    [Tooltip("Lực đẩy lên tối đa")]
    public float maxLaunchForce = 5f;

    [Header("Ground Detection")]
    [Tooltip("Layer của mặt đất để shard dừng lại")]
    public LayerMask groundLayer;
    [Tooltip("Offset dọc để kiểm tra va chạm xuống đất")]
    public float groundCheckOffset = 0.1f;
    [Tooltip("Chiều rộng vùng kiểm tra so với collider")]
    public float groundCheckWidthRatio = 0.9f;
    [Tooltip("Độ cao vùng kiểm tra xuống dưới collider")]
    public float groundCheckHeight = 0.1f;

    [Header("Ignore Collisions")]
    [Tooltip("Tags to ignore collisions with (Player, Enemy, Object)")]
    public string[] ignoreTags = new string[] { "Player", "Enemy", "Object" };

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        solidCollider = GetComponent<BoxCollider2D>();
        solidCollider.isTrigger = false;

        // Ignore collisions with specified tags
        foreach (string tag in ignoreTags)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objs)
            {
                Collider2D col = obj.GetComponent<Collider2D>();
                if (col != null)
                    Physics2D.IgnoreCollision(solidCollider, col);
            }
        }
    }

    void Start()
    {
        // Launch upward with random nudge
        float randomX = Random.Range(-horizontalNudge, horizontalNudge);
        float randomY = Random.Range(minLaunchForce, maxLaunchForce);
        rb.linearVelocity = new Vector2(randomX, randomY);
    }

    void FixedUpdate()
    {
        if (rb.bodyType == RigidbodyType2D.Static) return;

        if (IsGrounded() && rb.linearVelocity  .magnitude < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    private bool IsGrounded()
    {
        if (solidCollider == null) return false;
        Bounds bounds = solidCollider.bounds;
        Vector2 checkSize = new Vector2(bounds.size.x * groundCheckWidthRatio, groundCheckHeight);
        Vector2 checkCenter = new Vector2(bounds.center.x, bounds.min.y - groundCheckOffset);
        return Physics2D.OverlapBox(checkCenter, checkSize, 0f, groundLayer) != null;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (solidCollider == null) return;
        Bounds bounds = solidCollider.bounds;
        Vector2 size = new Vector2(bounds.size.x * groundCheckWidthRatio, groundCheckHeight);
        Vector2 center = new Vector2(bounds.center.x, bounds.min.y - groundCheckOffset);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
