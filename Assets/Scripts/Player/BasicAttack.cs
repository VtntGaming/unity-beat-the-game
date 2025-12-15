using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : MonoBehaviour
{
    [Header("Range Attack Parameters")]
    [SerializeField] public float rangeAttackCooldown = 0.25f;
    [SerializeField] private Transform flashPoint;
    [SerializeField] private int projectileDamage = 30;
    [SerializeField] private GameObject[] swordFlashes;

    [Header("Melee Attack Parameters")]
    [SerializeField] public float meleeAttackCooldown = 1f;
    [SerializeField] private float meleeRange = 1f;
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private float meleeColliderDistance = 0.5f;
    [SerializeField] private Collider2D boxCollider;
    [SerializeField] private LayerMask enemyLayer;

    // ===== THÊM CÁC BIẾN CHO COMBO =====
    [Header("Fire-Strength Combo")]
    [SerializeField] private GameObject chainExplosionPrefab;  // Kéo Prefab Vụ Nổ vào đây
    [SerializeField] private Transform comboSpawnPoint;       // "Object con" để xác định điểm bắt đầu
    [SerializeField] private float comboBaseDamage = 20f;       // Sát thương GỐC của combo
    [SerializeField] private LayerMask groundLayerForCombo; // Layer đất để kiểm tra
    [SerializeField] private float comboCooldown = 3.0f; // Cooldown riêng cho combo
    [SerializeField] private float firstSpawnHeightOffset = 0.5f;
    private float comboTimer = Mathf.Infinity;           // Biến đếm nội bộ

    [Header("Breakable Tile")]
    [SerializeField] private BreakableTilemap breakableTilemap;
    [SerializeField] private LayerMask breakableLayer;

    private Animator anim;
    private BasicMovement basicMovement;
    private Entity playerHealth;
    private BuffManager buffManager;
    private PlayerStats playerStats;

    private KnockbackController knockbackController; // Thêm tham chiếu đến KnockbackController

    public float rangeCooldownTimer = Mathf.Infinity;
    public float meleeCooldownTimer = Mathf.Infinity;

    void Start()
    {
        anim = GetComponent<Animator>();
        basicMovement = GetComponent<BasicMovement>();
        playerHealth = GetComponent<Entity>();

        // Lấy KnockbackController từ gameObject hoặc component khác
        knockbackController = GetComponent<KnockbackController>();

        GameObject SwordFlashHolder = GameObject.Find("SwordFlashHolder");
        List<GameObject> objList = new List<GameObject>();
        foreach (Transform child in SwordFlashHolder.transform)
        {
            objList.Add(child.gameObject);
        }

        swordFlashes = new GameObject[objList.Count];
        int i = 0;
        foreach (GameObject child in objList)
        {
            swordFlashes[i] = child;
            i++;
        }
        //Gọi BuffManager
        buffManager = GetComponent<BuffManager>();
        playerStats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (!playerHealth.dead)
        {
            rangeCooldownTimer += Time.deltaTime;
            meleeCooldownTimer += Time.deltaTime;
            comboTimer += Time.deltaTime;

            // Đòn tầm xa: kích hoạt bằng phím J
            if (Input.GetKeyDown(KeyCode.J) && rangeCooldownTimer > rangeAttackCooldown && basicMovement.canAttack())
            {
                Attack_range1();
                AudioManager.Sfx(Sound.AttackRanged);
            }

            // Đòn cận chiến: kích hoạt bằng phím K
            if (Input.GetKeyDown(KeyCode.K) && meleeCooldownTimer > meleeAttackCooldown && basicMovement.canAttack())
            {
                Attack_melee3();
                AudioManager.Sfx(Sound.AttackMelee);
            }
        }
    }

    // Xử lý đòn tầm xa
    private void Attack_range1()
    {
        anim.SetTrigger("Attack1");
        rangeCooldownTimer = 0;

        GameObject flash = swordFlashes[FindSwordFlash()];
        flash.transform.position = flashPoint.position;

        // ======== TÍNH TOÁN HỆ SỐ NHÂN TỔNG HỢP ========
        // 1. Lấy multiplier từ Buff (ví dụ: 1.0, 1.5...)
        float buffMultiplier = buffManager.GetDamageMultiplier();
        // 2. Lấy multiplier gốc từ PlayerStats (nếu có)
        float baseEquipMultiplier = (playerStats != null) ? playerStats.FinalDamageMultiplier : 1.0f;
        // 3. Lấy sát thương CỘNG THÊM từ Kiếm
        float equipBonusDamage = (playerStats != null) ? playerStats.FinalBonusDamage : 0f;

        // CÔNG THỨC MỚI: (Sát thương gốc đạn + Sát thương kiếm) * (Hệ số buff * Hệ số gốc)
        int finalDamage = Mathf.RoundToInt((projectileDamage + equipBonusDamage) * (buffMultiplier * baseEquipMultiplier));

        // Kiểm tra và in ra giá trị của scaleX của flashPoint
        Debug.Log("FlashPoint scaleX: " + flashPoint.localScale.x);

        // Lấy hướng từ flashPoint
        float flashPointDirection = Mathf.Sign(flashPoint.localScale.x); // Đảm bảo lấy đúng scaleX của flashPoint

        // Truyền hướng cho projectile
        Projectile proj = flash.GetComponent<Projectile>();
        if (proj != null)
        {
            // 👈 SỬ DỤNG HÀM MỚI
            proj.SetStats(flashPointDirection, finalDamage);
        }

        // Đảm bảo rằng hướng của projectile được cập nhật đúng sau khi flashPoint đã đặt đúng hướng
        Debug.Log("Projectile Direction: " + flashPointDirection);
    }


    private int FindSwordFlash()
    {
        for (int i = 0; i < swordFlashes.Length; i++)
        {
            if (!swordFlashes[i].activeInHierarchy)
                return i;
        }
        return 0;
    }

    // Xử lý đòn cận chiến
    private void Attack_melee3()
    {
        anim.SetTrigger("Attack3");
        meleeCooldownTimer = 0;
    }

    // Phương thức này được gọi thông qua Animation Event của animation "Attack2"
    public void MeleeAttack()
    {
        // ======== TÍNH TOÁN HỆ SỐ NHÂN TỔNG HỢP ========
        // Lấy multiplier từ buff
        float buffMultiplier = buffManager.GetDamageMultiplier();
        // Lấy multiplier gốc (từ buff...?) và multiplier của trang bị (từ playerstats)
        float equipMultiplier = playerStats.FinalDamageMultiplier;
        // LẤY SÁT THƯƠNG CỘNG THÊM TỪ KIẾM
        float equipBonusDamage = playerStats.FinalBonusDamage;

        int finalDamage = Mathf.RoundToInt((meleeDamage + equipBonusDamage) * (buffMultiplier * equipMultiplier));

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center + transform.right * meleeRange * transform.localScale.x * meleeColliderDistance,
            new Vector3(boxCollider.bounds.size.x * meleeRange, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
            0,
            Vector2.left,
            0,
            enemyLayer
        );

        if (hit.collider != null)
        {
            // Gây sát thương enemy
            Entity enemyHealth = hit.collider.GetComponent<Entity>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(finalDamage, Mathf.Sign(transform.localScale.x), gameObject, false);
            }
            else
            {
                // Gây sát thương crate
                CrateDestructible crate = hit.collider.GetComponent<CrateDestructible>();
                if (crate != null)
                {
                    crate.TakeDamage(finalDamage, Mathf.Sign(transform.localScale.x), gameObject, false);
                }
                else
                {
                    // Gây sát thương tilemap
                    BreakableTilemap breakable = hit.collider.GetComponent<BreakableTilemap>();
                    if (breakable != null)
                    {
                        breakable.BreakTileAt(hit.point); // 🔁 Sử dụng lại phá từng tile
                    }
                }
            }
        }
    }

    private bool StartChainExplosion()
    {
        // ===== BƯỚC 1: KIỂM TRA BUFF =====
        if (!buffManager.hasFireBuff || !buffManager.hasStrengthBuff)
        {
            return false; // Không có buff, báo thất bại (để MeleeAttack biết mà đánh thường)
        }

        // ===== BƯỚC 2: KIỂM TRA TIMER CỦA BẠN =====
        if (comboTimer < comboCooldown)
        {
            return false; // Timer chưa đạt, báo thất bại
        }

        // Timer đã đạt, TIẾP TỤC
        // ===== BƯỚC 3: RESET TIMER =====
        comboTimer = 0f;

        // 4. Tính sát thương & hướng
        // 4.1 ======== TÍNH TOÁN HỆ SỐ NHÂN TỔNG HỢP ========
        // Lấy multiplier từ buff
        float buffMultiplier = buffManager.GetDamageMultiplier();
        // Lấy multiplier gốc (từ buff...?) và multiplier của trang bị (từ playerstats)
        float equipMultiplier = playerStats.FinalDamageMultiplier;
        // LẤY SÁT THƯƠNG CỘNG THÊM TỪ KIẾM
        float equipBonusDamage = playerStats.FinalBonusDamage;

        int finalDamage = Mathf.RoundToInt((meleeDamage + equipBonusDamage) * (buffMultiplier * equipMultiplier));

        float direction = Mathf.Sign(transform.localScale.x);

        // 5. Lấy vị trí TRUNG TÂM của điểm spawn
        Vector3 centerPos = comboSpawnPoint.position;
        Vector3 spawnPos = centerPos; // Mặc định là trung tâm

        // Thử lấy collider của điểm spawn
        Collider2D spawnCollider = comboSpawnPoint.GetComponent<Collider2D>();
        if (spawnCollider != null)
        {
            // Nếu có collider, tính toán "cạnh dưới"
            // (bounds.center.y chính là centerPos.y)
            // (bounds.extents.y là một nửa chiều cao)
            spawnPos.y = spawnCollider.bounds.center.y - spawnCollider.bounds.extents.y;
        }
        else
        {
            // Nếu không có collider, chúng ta giả định spawnPos là điểm bạn muốn
            Debug.LogWarning("ComboSpawnPoint không có Collider2D, Raycast sẽ bắt đầu từ tâm.");
        }

        // 6. Kiểm tra đất (từ trung tâm raycast xuống)
        RaycastHit2D groundHit = Physics2D.Raycast(spawnPos, Vector2.down, 0.5f, groundLayerForCombo);

        if (groundHit.collider != null)
        {
            // 7. CÓ ĐẤT -> Sinh ra vụ nổ TẠI ĐIỂM CHẠM ĐẤT (cạnh dưới)
            Debug.Log("COMBO: Fire + Strength! Spawning chain 1.");

            // Spawn tại groundHit.point, chính là "cạnh dưới" mà bạn muốn
            Vector2 finalSpawnPoint = groundHit.point + (Vector2.up * firstSpawnHeightOffset);

            // Spawn tại vị trí MỚI (finalSpawnPoint) thay vì groundHit.point
            GameObject firstExplosion = Instantiate(chainExplosionPrefab, finalSpawnPoint, Quaternion.identity);

            // 8. Kích hoạt nó
            firstExplosion.GetComponent<ChainExplosion>().Trigger(finalDamage, 1, direction);

            // 9. Báo thành công
            return true;
        }
        else
        {
            // 10. KHÔNG CÓ ĐẤT
            Debug.Log("COMBO: Failed. Start point not on ground.");
            comboTimer = comboCooldown; // Hoàn lại cooldown
            return false; // Báo thất bại
        }
    }

    // Hiển thị phạm vi tấn công cận chiến trong Scene view (Debug)
    private void OnDrawGizmos()
    {
        if (boxCollider == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            boxCollider.bounds.center + transform.right * meleeRange * transform.localScale.x * meleeColliderDistance,
            new Vector3(boxCollider.bounds.size.x * meleeRange, boxCollider.bounds.size.y, boxCollider.bounds.size.z)
        );
    }
}
