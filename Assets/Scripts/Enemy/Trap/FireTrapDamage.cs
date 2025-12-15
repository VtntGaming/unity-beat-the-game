using UnityEngine;
using System.Collections.Generic;

public class FireTrapDamage : MonoBehaviour
{
    [Header("DOT Settings")]
    [Tooltip("Tên hiệu ứng DOT (Dùng làm Key trong Dictionary).")]
    [SerializeField] private string dotType = "Fire";

    [Tooltip("Sát thương mất mỗi lần tick (ví dụ: 10 máu).")]
    [SerializeField] private float damagePerTick = 10f;

    [Tooltip("Khoảng thời gian giữa các lần sát thương (ví dụ: 1.0 giây).")]
    [SerializeField] private float tickInterval = 1.0f;

    [Tooltip("Thời gian DOT được gia hạn/thêm vào mỗi lần va chạm.")]
    [SerializeField] private float dotDurationOnHit = 10f;

    // Dùng HashSet để tránh gọi DOT nhiều lần cho cùng 1 Player
    private HashSet<GameObject> affectedPlayers = new HashSet<GameObject>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Khi player mới bước vào vùng lửa
        if (other.CompareTag("Player") && !affectedPlayers.Contains(other.gameObject))
        {
            affectedPlayers.Add(other.gameObject);

            var playerHealth = other.GetComponent<Entity>();
            var dotController = other.GetComponent<DamageOverTimeController>();

            if (playerHealth != null && !playerHealth.dead && dotController != null)
            {
                dotController.ApplyDot(
                    dotType,
                    damagePerTick,
                    tickInterval,
                    dotDurationOnHit,
                    this
                );

                Debug.Log($"🔥 FireTrap applied DOT to {other.name} for {dotDurationOnHit}s");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Khi player rời khỏi vùng lửa → cho phép nhận DOT lại lần sau
        if (other.CompareTag("Player"))
        {
            affectedPlayers.Remove(other.gameObject);
            Debug.Log($"🔥 {other.name} exited fire trap zone. DOT can be reapplied later.");
        }
    }
}
