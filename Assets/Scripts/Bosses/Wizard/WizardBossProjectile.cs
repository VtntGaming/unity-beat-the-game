using UnityEngine;

public class WizardProjectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    public float speed = 10f;
    public float damage = 15f;
    public float lifeTime = 5f; // Thời gian tự hủy nếu không trúng gì

    // Đã xóa biến explosionEffect vì dùng Animator có sẵn

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col; // Để tắt va chạm sau khi nổ
    private bool hasExploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        // Hủy đạn sau X giây nếu bắn trượt ra ngoài map
        Destroy(gameObject, lifeTime);
    }

    // Hàm này được AttackAI gọi khi Spawn
    public void Launch(Vector2 direction)
    {
        // Xoay đạn theo hướng bay
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Gán vận tốc (Unity 6: linearVelocity)
        if (rb != null) rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Nếu đã nổ rồi thì không xử lý nữa (tránh gây dmg 2 lần)
        if (hasExploded) return;

        // Bỏ qua va chạm với Boss và các quái khác
        if (hitInfo.CompareTag("Enemy") || hitInfo.GetComponent<Entity>() is BossEntity) return;

        // Kiểm tra va chạm
        if (hitInfo.CompareTag("Player") || hitInfo.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Nếu trúng Player -> Gây damage
            if (hitInfo.CompareTag("Player"))
            {
                Entity player = hitInfo.GetComponent<Entity>();
                if (player != null)
                {
                    // Tính hướng knockback (đạn bay từ trái hay phải tới)
                    float dir = transform.position.x < hitInfo.transform.position.x ? 1 : -1;

                    // Gây dmg (canBeBlocked=true, isProjectile=true)
                    player.TakeDamage(damage, dir, gameObject, true, true);
                }
            }

            // Kích hoạt nổ
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;

        // 1. Dừng bay ngay lập tức
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 2. Tắt Collider ngay để không gây dmg thêm lần nữa
        if (col != null) col.enabled = false;

        // 3. Chuyển Animator sang trạng thái Nổ
        if (anim != null)
        {
            anim.SetTrigger("Explode");

            // ❌ ĐÃ XÓA DÒNG: Destroy(gameObject, 0.5f);
            // Chúng ta sẽ để Animation Event gọi hàm DestroyProjectile() bên dưới
        }
        else
        {
            // Trường hợp lỗi không có Animator thì hủy luôn cho đỡ rác
            Destroy(gameObject);
        }
    }

    // =========================================================
    // HÀM MỚI: DÙNG CHO ANIMATION EVENT
    // (Gắn vào frame cuối cùng của animation nổ)
    // =========================================================
    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}