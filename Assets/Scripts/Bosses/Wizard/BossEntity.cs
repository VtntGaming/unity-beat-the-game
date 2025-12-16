using UnityEngine;

public class BossEntity : Entity
{
    [Header("Boss Poise System")]
    [SerializeField] private float maxPoise = 100f;
    [SerializeField] private float poiseRecoverySpeed = 10f;
    [SerializeField] private float poiseRecoveryDelay = 3f;

    private float currentPoise;
    private float poiseTimer;

    // Cache các script AI để tắt cho lẹ
    private MovementAI movementAI;
    private AttackAI attackAI;
    private Rigidbody2D rb;

    private void Start()
    {
        currentPoise = maxPoise;
        // Tự động tìm component
        movementAI = GetComponent<MovementAI>();
        attackAI = GetComponent<AttackAI>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Logic hồi phục Poise
        // (Chúng ta không cần override Update của cha, nhưng cần chạy logic riêng)
        if (!dead && poiseTimer <= 0 && currentPoise < maxPoise)
        {
            currentPoise += poiseRecoverySpeed * Time.deltaTime;
            currentPoise = Mathf.Clamp(currentPoise, 0, maxPoise);
        }
        else
        {
            poiseTimer -= Time.deltaTime;
        }
    }

    public override void Deactivate()
    {
        unlockPortal();
        base.Deactivate();
    }

    private void unlockPortal()
    {
        GameObject portal = GameObject.Find("InteractObject")?.transform.Find("Portal").gameObject;
        if (portal != null)
        {
            portal.SetActive(true);
        }
    }

    // Ghi đè hàm TakeDamage của cha
    public override void TakeDamage(float _damage, float attackerDirection, GameObject attacker = null, bool isProjectile = false, bool canBeBlocked = true)
    {
        // 1. Tính toán Poise trước khi trừ máu
        if (!dead)
        {
            // Trừ Poise (dựa trên sát thương nhận vào)
            // Bạn có thể chỉnh công thức này (ví dụ mỗi hit trừ 10 điểm cứng)
            float poiseDamage = _damage;
            currentPoise -= poiseDamage;
            poiseTimer = poiseRecoveryDelay;

            // Debug.Log($"Boss Poise: {currentPoise}/{maxPoise}");

            // 2. Kiểm tra Vỡ Giáp (Stun)
            if (currentPoise <= 0)
            {
                // Vỡ giáp! Kích hoạt Animation STUN
                // Lưu ý: Trong Animator của Boss, hãy đổi tên Trigger "Hurt" thành "Stun"
                // Để tránh việc script cha (Entity) tự động gọi Hurt.
                GetComponent<Animator>().SetTrigger("Stun");

                Debug.Log("<color=red>BOSS BỊ CHOÁNG (POISE BROKEN)!</color>");

                // Reset Poise
                currentPoise = maxPoise;

                // Ngắt đòn đánh của Boss AI (nếu có)
                var movementAI = GetComponent<MovementAI>();
                if (movementAI != null) movementAI.StopImmediate();
            }
            else
            {
                // Nếu chưa vỡ giáp -> Chỉ nháy đỏ (Hiệu ứng này nằm ở base.TakeDamage)
                // Nhưng Boss sẽ KHÔNG bị khựng animation vì Animator không có trigger tên "Hurt"
            }
        }

        // 3. Gọi về script cha để thực hiện trừ máu, hiển thị damage, chết, drop đồ...
        base.TakeDamage(_damage, attackerDirection, attacker, isProjectile, canBeBlocked);

        // 4. KIỂM TRA CHẾT (QUAN TRỌNG: GHI ĐÈ LOGIC CŨ CỦA CHA ĐỂ TẮT AI)
        if (currentHealth <= 0)
        {
            DisableBossActions();
        }
    }
    private void DisableBossActions()
    {
        // Tắt AttackAI trước để nó không gọi lệnh mới
        if (attackAI != null)
        {
            attackAI.StopAllCoroutines(); // Dừng mọi suy nghĩ
            attackAI.enabled = false;     // Tắt não
        }

        // Tắt MovementAI sau
        if (movementAI != null)
        {
            movementAI.StopImmediate();   // Dừng di chuyển ngay lập tức
            movementAI.StopAllCoroutines();
            movementAI.enabled = false;   // Tắt chân
        }

        // Dừng vật lý để không bị trượt đi
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // Khóa cứng vị trí lại (hoặc Dynamic tùy logic rơi của bạn)
        }

        // Tắt Collider để không chặn đường Player (Tùy chọn)
        // GetComponent<Collider2D>().enabled = false; 
    }
}