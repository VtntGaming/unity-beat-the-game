using UnityEngine;
using System.Collections.Generic; // Không cần thiết nếu xóa HashSet

public class EnemyDamageDot : MonoBehaviour
{
    [Header("DOT Settings")]
    public string dotType = "Poison";
    public float damagePerTick = 5f;
    public float tickInterval = 1f;
    public float dotDurationOnHit = 5f;

    // *** ĐÃ XÓA private HashSet<GameObject> affectedPlayers; ***

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ kiểm tra Tag
        if (!other.CompareTag("Player"))
            return;

        // *** ĐÃ XÓA affectedPlayers.Add(other.gameObject); ***

        DamageOverTimeController dotController = other.GetComponent<DamageOverTimeController>();
        if (dotController != null)
        {
            // Gọi ApplyDot mỗi lần chạm
            dotController.ApplyDot(dotType, damagePerTick, tickInterval, dotDurationOnHit, this);
        }
    }

    // *** ĐÃ XÓA OnTriggerExit2D hoàn toàn ***
}