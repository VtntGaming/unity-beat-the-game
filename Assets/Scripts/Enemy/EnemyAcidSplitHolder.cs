using UnityEngine;

public class EnemyAcidSplitHolder : EnermyEntity
{
    [SerializeField] private Transform enemy;

    private void LateUpdate()  // ⭐ Dùng LateUpdate
    {
        // Kiểm tra xem enemy còn tồn tại không
        if (enemy == null)
        {
            // Nếu enemy đã chết/bị hủy, thì hủy luôn object này để tránh lỗi
            Destroy(gameObject);
            return;
        }

        transform.localScale = enemy.localScale;
    }
}