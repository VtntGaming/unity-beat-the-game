using UnityEngine;
using System.Collections;

public class KnockbackController : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForceX = 5f;  // Lực đẩy ngang
    [SerializeField] private float enemyKnockbackDuration = 0.5f; // Thời gian bất động cho enemy
    [SerializeField] private Behaviour[] disableComponents; // Component sẽ tạm tắt

    [SerializeField] private BasicMovement basicMovement;

    private Rigidbody2D rb;
    private bool isKnockingBack = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (basicMovement == null)
            basicMovement = GetComponent<BasicMovement>();
    }

    /// <summary>
    /// Gây hiệu ứng knockback theo hướng của attacker.
    /// attackerDirection: 1 (attacker quay phải), -1 (attacker quay trái)
    /// </summary>
    public void ApplyKnockback(float attackerDirection, bool isProjectile)
    {
        if (rb == null || isKnockingBack) return;

        isKnockingBack = true;

        // Tắt các component tạm thời
        foreach (Behaviour comp in disableComponents)
        {
            if (comp != null)
                comp.enabled = false;
        }

        // Reset vận tốc trước khi áp lực mới
        rb.linearVelocity = Vector2.zero;

        // Đặt góc knockback (ví dụ góc 45 độ)
        float knockbackAngle = 45f;
        float knockbackForce = knockbackForceX;  // Có thể điều chỉnh mạnh yếu của lực đẩy

        // Chuyển đổi góc sang radian
        float angleRad = Mathf.Deg2Rad * knockbackAngle;

        // Tính toán lực đẩy theo trục X và Y
        float forceX = knockbackForce * Mathf.Cos(angleRad) * Mathf.Sign(attackerDirection);
        float forceY = knockbackForce * Mathf.Sin(angleRad);

        // Tạo vector lực knockback
        Vector2 force = new Vector2(forceX, forceY);
        rb.AddForce(force, ForceMode2D.Impulse);

        // Bắt đầu routine để phục hồi sau knockback
        StartCoroutine(KnockbackRoutine());
    }

    private IEnumerator KnockbackRoutine()
    {
        // Không cần phải kiểm tra "IsGround", sẽ chỉ dừng khi thời gian knockback kết thúc
        yield return new WaitForSeconds(enemyKnockbackDuration);

        // Kích hoạt lại các component sau khi knockback kết thúc
        foreach (Behaviour comp in disableComponents)
        {
            if (comp != null)
                comp.enabled = true;
        }

        isKnockingBack = false;
    }
}
