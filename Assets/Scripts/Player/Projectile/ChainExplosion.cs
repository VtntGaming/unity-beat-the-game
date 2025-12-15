using UnityEngine;
using System.Collections; // <-- Thêm thư viện này để dùng Coroutine

// Tự động thêm Collider2D và đảm bảo nó là trigger
[RequireComponent(typeof(Collider2D))]
public class ChainExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float radius = 1.5f;       // Bán kính vụ nổ
    [SerializeField] private LayerMask enemyLayer;    // Layer của Enemy
    [SerializeField] private GameObject explosionEffect; // Gắn Particle/Animation effect vào đây

    [Header("Chain Logic")]
    [SerializeField] private GameObject explosionPrefab;     // Kéo CHÍNH PREFAB này vào đây
    [SerializeField] private int maxChainCount = 5;         // Giới hạn chuỗi nổ
    [SerializeField] private float nextSpawnOffset = 1.5f;    // Khoảng cách ngang tới vụ nổ kế
    [SerializeField] private float chainDelay = 0.15f;      // ===== ĐỘ TRỄ GIỮA CÁC VỤ NỔ =====
    [SerializeField] private float spawnHeightOffset = 0.5f; // ===== NÂNG VỊ TRÍ LỬA LÊN =====
    [SerializeField] private float groundCheckDistance = 1.5f;  // Khoảng cách kiểm tra đất
    [SerializeField] private LayerMask groundLayer;         // Layer của mặt đất

    private void Awake()
    {
        // ===== ĐẢM BẢO OBJECT CÓ THỂ ĐI XUYÊN QUA =====
        // (Logic sát thương dùng OverlapCircle nên không bị ảnh hưởng)
        GetComponent<Collider2D>().isTrigger = true;
    }

    // Hàm được gọi bởi BasicAttack (cho vụ nổ 1) hoặc bởi vụ nổ trước đó
    public void Trigger(float damage, int currentCount, float direction)
    {
        // 1. Kích hoạt hiệu ứng, âm thanh
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        AudioManager.Sfx(Sound.AttackMelee);

        // 2. Gây sát thương (Kiểm tra "Gizmos" như bạn muốn)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            Entity enemyHealth = hit.GetComponent<Entity>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, 0, gameObject, false);
            }
        }

        // 3. ===== LOGIC HỦY ĐÃ BỊ XÓA KHỎI ĐÂY =====
        //    Destroy(gameObject, lifeTime); // <--- Xóa dòng này

        // 4. KIỂM TRA ĐỂ SINH RA VỤ NỔ TIẾP THEO
        if (currentCount >= maxChainCount)
        {
            return; // Đã đạt giới hạn, dừng chuỗi
        }

        // 5. ===== BẮT ĐẦU COROUTINE ĐỂ TẠO TRỄ =====
        // Thay vì sinh ra ngay, chúng ta gọi một hàm chờ
        StartCoroutine(SpawnNextAfterDelay(damage, currentCount + 1, direction));
    }

    // ===== HÀM MỚI: CHỜ VÀ SINH RA VỤ NỔ KẾ TIẾP =====
    private IEnumerator SpawnNextAfterDelay(float damage, int nextCount, float direction)
    {
        // 1. Chờ theo độ trễ bạn set
        yield return new WaitForSeconds(chainDelay);

        // 2. Tính vị trí để *kiểm tra* (vẫn là ở trên không)
        Vector3 nextCheckPos = transform.position + (Vector3.right * direction * nextSpawnOffset);

        // 3. Kiểm tra mặt đất tại vị trí đó
        RaycastHit2D groundHit = Physics2D.Raycast(nextCheckPos, Vector2.down, groundCheckDistance, groundLayer);

        if (groundHit.collider != null)
        {
            // CÓ ĐẤT!
            Debug.Log($"Chain {nextCount - 1} spawning next at {groundHit.point}");

            // 4. ===== TÍNH VỊ TRÍ SPAWN MỚI (ĐÃ NÂNG LÊN) =====
            Vector2 spawnPoint = groundHit.point + (Vector2.up * spawnHeightOffset);

            // 5. Spawn tại điểm đã nâng lên
            GameObject nextExplosionObj = Instantiate(explosionPrefab, spawnPoint, Quaternion.identity);

            // 6. Kích hoạt nó, tăng count lên
            nextExplosionObj.GetComponent<ChainExplosion>().Trigger(damage, nextCount, direction);
        }
        else
        {
            // Không có đất, chuỗi bị ngắt
            Debug.Log($"Chain {nextCount - 1} stopped. No ground found.");
        }
    }

    // ===== HÀM MỚI: ĐỂ ANIMATION EVENT GỌI =====
    // Hàm này được gọi ở frame cuối của animation nổ
    public void Despawn()
    {
        Destroy(gameObject);
    }

    // Vẽ bán kính trong Editor để bạn dễ căn chỉnh
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius); // Vòng tròn sát thương

        // Vẽ tia kiểm tra đất
        Gizmos.color = Color.blue;
        // Dùng transform.right để tia gizmos luôn hướng về phía trước của prefab
        Vector3 nextPos = transform.position + (transform.right * nextSpawnOffset);
        Gizmos.DrawLine(nextPos, nextPos + Vector3.down * groundCheckDistance);

        // ===== VẼ VỊ TRÍ SPAWN MỚI (MÀU XANH LÁ) =====
        Gizmos.color = Color.green;
        Vector3 spawnPreviewPos = nextPos + Vector3.down * groundCheckDistance + Vector3.up * spawnHeightOffset;
        Gizmos.DrawWireSphere(spawnPreviewPos, 0.2f);
    }
}