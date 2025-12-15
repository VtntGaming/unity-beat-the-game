using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DamageOverTimeController : MonoBehaviour
{
    // ====================================================================
    // 1. CẤU TRÚC DỮ LIỆU DOT
    // ====================================================================

    /// <summary>
    /// Định nghĩa các thuộc tính của một hiệu ứng sát thương duy trì (DOT).
    /// </summary>
    public class DotEffect
    {
        public float damagePerTick;      // Sát thương mỗi lần tick
        public float tickInterval;       // Khoảng thời gian giữa các tick
        public float durationRemaining;  // Thời gian hiệu lực còn lại
        public MonoBehaviour sourceScript; // Script nguồn gây sát thương
        public float lastTickTime;       // Thời điểm tick gần nhất
        public float lastRefreshTime;    // Thời điểm lần cuối được refresh

        public DotEffect(float damage, float interval, float duration, MonoBehaviour source)
        {
            damagePerTick = damage;
            tickInterval = interval;
            durationRemaining = duration;
            sourceScript = source;
            lastTickTime = Time.time;
            lastRefreshTime = Time.time;
        }
    }

    // ====================================================================
    // 2. LOGIC QUẢN LÝ DOT
    // ====================================================================

    private Dictionary<string, DotEffect> activeDots = new Dictionary<string, DotEffect>();
    private Entity playerHealth;

    void Start()
    {
        playerHealth = GetComponent<Entity>();
        if (playerHealth == null)
        {
            Debug.LogError("❌ Health component not found on player! DOT system cannot function.");
        }
    }

    /// <summary>
    /// Áp dụng hoặc làm mới hiệu ứng DOT (gọi từ FireTrapDamage.cs, EnemyPoison.cs,...)
    /// </summary>
    public void ApplyDot(string dotType, float damagePerTick, float tickInterval, float duration, MonoBehaviour source)
    {
        if (playerHealth == null) return;

        const float refreshCooldown = 2f; // 👈 Thời gian cần chờ trước khi được phép reset (giây)

        // Nếu DOT cùng loại đã tồn tại
        if (activeDots.ContainsKey(dotType))
        {
            var dot = activeDots[dotType];
            float timeSinceLastRefresh = Time.time - dot.lastRefreshTime;

            // Nếu đã qua thời gian cooldown → cho phép reset lại thời gian DOT
            if (timeSinceLastRefresh >= refreshCooldown)
            {
                dot.durationRemaining = duration;
                dot.lastRefreshTime = Time.time;

                Debug.Log($"🔁 DOT '{dotType}' reset sau {timeSinceLastRefresh:F1}s. Thời lượng mới = {duration:F1}s");
            }
            else
            {
                Debug.Log($"⏳ DOT '{dotType}' đang trong cooldown ({timeSinceLastRefresh:F1}s < {refreshCooldown}s) → bỏ qua reset");
            }

            return; // Không tạo DOT mới
        }

        // Nếu DOT chưa có → tạo mới
        DotEffect newDot = new DotEffect(damagePerTick, tickInterval, duration, source);
        activeDots.Add(dotType, newDot);
        StartCoroutine(ProcessDot(dotType));
        Debug.Log($"🔥 DOT '{dotType}' được áp dụng (damage/tick={damagePerTick}, tickInterval={tickInterval}, duration={duration})");
    }

    /// <summary>
    /// Coroutine xử lý gây sát thương duy trì cho từng loại DOT.
    /// </summary>
    private IEnumerator ProcessDot(string dotType)
    {
        if (playerHealth == null || !activeDots.ContainsKey(dotType)) yield break;

        DotEffect dot = activeDots[dotType];
        float startTime = Time.time;

        // Tick đầu tiên ngay lập tức
        playerHealth.TakeDamage(dot.damagePerTick, 0f, null, false);
        Debug.Log($"[{dotType}] +{Time.time - startTime:F2}s | Tick đầu tiên: -{dot.damagePerTick} HP (Còn {dot.durationRemaining:F1}s)");

        dot.durationRemaining -= dot.tickInterval;

        // Tick các lần tiếp theo
        while (activeDots.ContainsKey(dotType) && dot.durationRemaining > 0)
        {
            yield return new WaitForSeconds(dot.tickInterval);

            if (playerHealth != null && !playerHealth.dead)
            {
                playerHealth.TakeDamage(dot.damagePerTick, 0f, null, false);
                Debug.Log($"[{dotType}] +{Time.time - startTime:F2}s | Tick: -{dot.damagePerTick} HP (Còn {dot.durationRemaining:F1}s)");
            }

            dot.durationRemaining -= dot.tickInterval;
        }

        // Kết thúc DOT
        if (activeDots.ContainsKey(dotType))
        {
            activeDots.Remove(dotType);
            Debug.Log($"✅ [{dotType}] KẾT THÚC sau {Time.time - startTime:F2}s");
        }
    }

}
