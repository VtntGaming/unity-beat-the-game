using UnityEngine;

public class MeleeEnemy : EnermyEntity
{
    [Header("Attack Parameters")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float range;
    [SerializeField] private int damage;

    [Header("Collider Parameters")]
    [SerializeField] private float colliderDistance;
    [SerializeField] private BoxCollider2D boxCollider;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;

    //[Header("Jump Attack Parameters")]
    //[SerializeField] private float attackJumpForce;
    //[SerializeField] private float jumpAngle = 45f; // Góc nhảy 45 độ

    private float cooldownTimer = Mathf.Infinity;

    // References
    private Animator anim;
    private Entity playerHealth;
    private EnemyPatrol enemyPatrol;
    private Rigidbody2D rb;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        enemyPatrol = GetComponentInParent<EnemyPatrol>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (PlayerInSight())
        {
            // Ngăn quái tấn công nếu player đang lướt
            BasicMovement movement = playerHealth?.GetComponent<BasicMovement>();
            if (movement != null && movement.IsDashing)
            {
                return;
            }

            if (cooldownTimer >= attackCooldown)
            {
                cooldownTimer = 0;
                anim.SetTrigger("jumpAttack");
                AudioManager.Sfx(Sound.EnemyMelee);
            }
        }

        if (enemyPatrol != null)
            enemyPatrol.enabled = !PlayerInSight();
    }


    private bool PlayerInSight()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
            0,
            Vector2.left,
            0,
            playerLayer);

        if (hit.collider != null)
        {
            playerHealth = hit.transform.GetComponent<Entity>();
            if (playerHealth == null)
            {
                // Nếu không có component Health, trả về false
                return false;
            }
        }

        return hit.collider != null;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z)
        );
    }

    //// Hàm này được gọi từ Animation Event khi hoạt ảnh "jumpAttack" đạt đến frame cần thực hiện nhảy
    //public void JumpAttack()
    //{
    //    // Xác định hướng nhảy theo scale (1: sang phải, -1: sang trái)
    //    float jumpDir = transform.localScale.x;
    //    // Tính góc nhảy theo radian
    //    float angleRad = Mathf.Deg2Rad * jumpAngle;
    //    // Hệ số nhân lực nhảy (có thể điều chỉnh để phù hợp)
    //    float k = 2.0f;
    //    // Tính vận tốc theo trục X và Y
    //    float vx = attackJumpForce * k * Mathf.Cos(angleRad) * jumpDir;
    //    float vy = attackJumpForce * k * Mathf.Sin(angleRad);
    //    rb.linearVelocity = new Vector2(vx, vy);

    //    // Bạn có thể gọi hàm DamagePlayer() tại một Animation Event khác (ví dụ, khi đạt frame "đánh trúng")
    //    // hoặc sử dụng trigger trong quá trình va chạm.
    //    // anim.SetTrigger("Attack"); // Nếu muốn kích hoạt hoạt ảnh tấn công khi nhảy
    //}

    // Hàm này gọi để gây sát thương cho người chơi
    // Bạn có thể gọi nó thông qua Animation Event khi slime "chạm" trúng mục tiêu
    public void DamagePlayer()
    {
        if (playerHealth != null)
        {
            float direction = Mathf.Sign(transform.localScale.x);
            playerHealth.TakeDamage(damage, direction, gameObject, false); // false = not a projectile
        }
        else
        {
            Debug.LogError("PlayerHealth is null! Cannot damage player.");
        }
    }
}
