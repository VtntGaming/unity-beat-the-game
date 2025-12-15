using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Parameters")]
    [SerializeField] private float speed = 10f;
    //[SerializeField] private int damage = 50;
    [SerializeField] private float lifetimeLimit = 5f;

    private float direction;
    private float lifetime;
    private bool hit;
    //private float damageMultiplier = 1.0f;

    private BoxCollider2D boxCollider;
    private Animator anim;
    private Vector3 initialScale;

    // thêm mới
    private int finalDamage;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        initialScale = transform.localScale;
    }

    private void OnEnable()
    {
        // Đặt lại trạng thái
        lifetime = 0;
        hit = false;
        boxCollider.enabled = true;

        // Đặt lại hướng và scale
        transform.localScale = new Vector3(initialScale.x * Mathf.Sign(direction), initialScale.y, initialScale.z);
    }

    private void Update()
    {
        if (hit) return;

        // Di chuyển
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime, Space.World);

        // Hủy nếu quá thời gian sống
        lifetime += Time.deltaTime;
        if (lifetime > lifetimeLimit)
        {
            Deactivate();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Lọc các va chạm không mong muốn
        if (collision.CompareTag("EffectZone") || collision.CompareTag("Projectile"))
            return;

        if (collision.GetComponent<Projectile>() != null || collision.GetComponent<EnemyProjectile>() != null)
            return;

        // Nếu đã va chạm rồi thì không xử lý nữa
        if (hit)
            return;

        // Đánh dấu đã va chạm
        hit = true;
        anim.SetTrigger("Explode");
        AudioManager.Sfx(Sound.PlayerProjectileHit);
        boxCollider.enabled = false; // Tắt collider ngay lập tức

        // (Nếu đạn của bạn di chuyển bằng Rigidbody, hãy dừng nó lại)
        // if (rb != null) rb.velocity = Vector2.zero;

        // ===== TÍNH TOÁN SÁT THƯƠNG CUỐI CÙNG =====
        //int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

        // Gây sát thương cho enemy
        if (collision.CompareTag("Enemy"))
        {
            Entity enemyHealth = collision.GetComponent<Entity>();
            if (enemyHealth != null)
            {
                // Sử dụng finalDamage
                enemyHealth.TakeDamage(finalDamage, direction, gameObject, true);
            }
        }

        // Phá crate
        CrateDestructible crate = collision.GetComponent<CrateDestructible>();
        if (crate != null)
        {
            // Sử dụng finalDamage
            crate.TakeDamage(finalDamage, direction, gameObject, true);
        }

        // Phá tile tại vị trí va chạm
        BreakableTilemap breakable = collision.GetComponent<BreakableTilemap>();
        if (breakable != null)
        {
            Vector2 hitPos = transform.position;
            breakable.BreakTileAt(hitPos);
        }

        // Hủy object này sau 0.5 giây để animation "Explode" kịp chạy
        boxCollider.enabled = false;
    }


    /// <summary>
    /// Thiết lập hướng bay của projectile.
    /// </summary>
    /// 
    public void SetStats(float _direction, int _damage)
    {
        direction = Mathf.Sign(_direction);
        finalDamage = _damage; // 👈 Nhận sát thương từ BasicAttack
        gameObject.SetActive(true);
    }

    //public void SetDirection(float _direction)
    //{
    //    direction = Mathf.Sign(_direction); // Đảm bảo chỉ có -1 hoặc 1
    //    gameObject.SetActive(true);
    //}

    //public void SetMultiplier(float multiplier)
    //{
    //    damageMultiplier = multiplier;
    //}

    /// <summary>
    /// Hủy kích hoạt projectile.
    /// </summary>
    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
