using UnityEngine;
using System.Collections.Generic;

public class BuffManager : MonoBehaviour
{
    private Dictionary<BuffType, bool> activeBuffs = new Dictionary<BuffType, bool>();

    [Header("Buff Settings")]
    // ===== THÊM DÒNG NÀY =====
    // Giờ bạn có thể chỉnh ở Inspector: 1.1 = 10%, 1.15 = 15%, 1.2 = 20%
    [SerializeField] private float strengthMultiplier = 1.1f;

    public bool hasStrengthBuff { get; private set; } = false; // Biến này theo dõi trạng thái buff Strength
    public bool hasFireBuff { get; private set; } = false;

    // Áp dụng buff cho player
    public void ApplyBuff(BuffType buff)
    {
        if (!activeBuffs.ContainsKey(buff))
        {
            activeBuffs.Add(buff, true);
            Debug.Log("Buff applied: " + buff.ToString());

            // Xử lý từng loại buff
            switch (buff)
            {
                case BuffType.DoubleJump:
                    BasicMovement movement = GetComponent<BasicMovement>();
                    if (movement != null)
                        movement.hasDoubleJumpUpgrade = true;
                    break;

                case BuffType.DashCooldownReduce:
                    BasicMovement move = GetComponent<BasicMovement>();
                    if (move != null)
                    {
                        float reduction = 2f;
                        move.dashCooldownTime = Mathf.Max(move.dashCooldownTime - reduction, move.minCooldown);
                        move.hasDashCooldownBuff = true; // Đánh dấu là đang có buff
                        Debug.Log($"Dash cooldown reduced by {reduction}. New cooldown: {move.dashCooldownTime}");
                    }
                    break;

                case BuffType.FireElement:
                    hasFireBuff = true;
                    Debug.Log("Fire element buff applied.");
                    break;
                // Các buff khác
                // case BuffType.SpeedBoost: ...
                // case BuffType.Invincibility: ...
                case BuffType.Strength:
                    hasStrengthBuff = true; // Bật cờ Strength lên
                    Debug.Log("Strength buff applied!");
                    break;
            }
        }
    }
    public Dictionary<BuffType, bool> GetActiveBuff()
    {
        return activeBuffs;
    }

    // Xóa tất cả buffs khi chết
    public void RemoveAllBuffs()
    {
        activeBuffs.Clear();
        Debug.Log("All buffs removed on death.");

        // Reset trạng thái các buff khác
        BasicMovement movement = GetComponent<BasicMovement>();
        if (movement != null)
        {
            // Hủy buff Double Jump (nếu có)
            movement.hasDoubleJumpUpgrade = false;

            // Nếu có buff giảm cooldown Dash, khôi phục cooldown gốc
            if (movement.hasDashCooldownBuff)
            {
                movement.dashCooldownTime = movement.originalDashCooldownTime;  // Khôi phục cooldown gốc
                movement.hasDashCooldownBuff = false;  // Tắt trạng thái buff giảm cooldown
                Debug.Log("Dash cooldown buff expired due to death.");
            }
        }

        if (hasStrengthBuff)
        {
            hasStrengthBuff = false;
            Debug.Log("Strength buff expired due to death.");
        }

        if (hasFireBuff)
        {
            hasFireBuff = false;
            Debug.Log("Fire buff expired due to death.");
        }
        // Reset các buff khác nếu cần...
    }
    public float GetDamageMultiplier()
    {
        // Thay vì 1.1f, ta dùng biến đã khai báo ở trên
        return hasStrengthBuff ? strengthMultiplier : 1.0f;
    }

}

public enum BuffType
{
    DoubleJump,
    DashCooldownReduce,
    FireElement,      // thêm loại buff nguyên tố Hỏa
    Strength,         // Buff tăng sức mạnh tấn công
    // Các buff khác
    // SpeedBoost,
    // Invincibility,
}
