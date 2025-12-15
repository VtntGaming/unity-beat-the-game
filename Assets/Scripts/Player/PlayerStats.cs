using UnityEngine;
using System.Linq;

[RequireComponent(typeof(PlayerController))]
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseArmour = 20f;
    [SerializeField] private float baseDamageMultiplier = 1.0f; // Multiplier gốc (dành cho Buff)

    // ======== SOCKET RỖNG ========
    [Header("Equipped Items (Sockets)")]
    public EquipmentItem equippedSword;
    public EquipmentItem equippedArmor;

    // ======== PROGRESSION ========
    public PlayerProgression plrProgression = new PlayerProgression();

    // =============================

    // --- CÁC CHỈ SỐ CUỐI CÙNG (ĐÃ THAY ĐỔI) ---
    public float FinalArmour { get; private set; } // Giáp (Base + Đồ)

    // ⚠️ THAY ĐỔI LỚN ⚠️
    // Đây là sát thương CỘNG THÊM từ vũ khí (ví dụ: 15.51)
    public float FinalBonusDamage { get; private set; }
    // Multiplier này giờ CHỈ DÀNH CHO BUFF
    public float FinalDamageMultiplier { get; private set; }

    // Tham chiếu
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        RecalculateStats();
    }

    private void Start()
    {
        LogStats();
    }

    // ===== TEST LOGIC (GIỮ NGUYÊN) =====
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("--- [THỬ NÂNG CẤP KIẾM] ---");
            if (equippedSword != null)
            {
                equippedSword.AttemptUpgrade(playerController);
                RecalculateStats(); // Tính lại chỉ số sau khi nâng cấp
                LogStats();
                updateEquipped();
            }
        }
        // ... (Nâng cấp giáp tương tự) ...
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("--- [THỬ NÂNG CẤP GIÁP] ---");
            if (equippedArmor != null)
            {
                equippedArmor.AttemptUpgrade(playerController);
                RecalculateStats();
                updateEquipped();
            }
        }
    }

    void updateEquipped()
    {
        try
        {
            EquipmentUI ui = getEquipment();
            if (ui != null)
            {
                ui.UpdateEquipped();
            }
            else
            {
                throw new UnityException("EquipmentUI could not be found");
            }
        }
        catch (UnityException ex) {
            Debug.LogWarning("Could not update equip UI: " + ex.Message);
        }
    }

    EquipmentUI getEquipment()
    {
        EquipmentUI ui = GameObject.FindAnyObjectByType<EquipmentUI>();
        return ui;
    }

    /// <summary>
    /// Thử trang bị một item.
    /// Trả về TRUE nếu trang bị thành công (hoặc cho vào túi).
    /// Trả về FALSE nếu ổ đã đầy.
    /// </summary>
    public bool EquipItem(EquipmentItem itemToEquip)
    {
        if (itemToEquip == null || !itemToEquip.HasItem())
        {
            return false;
        }

        EquipmentType type = itemToEquip.GetItemType(); // 👈 Cần thêm hàm này vào EquipmentInstance

        // 1. LOGIC TRANG BỊ KIẾM
        if (type == EquipmentType.Sword)
        {
            // Kiểm tra "socket" kiếm có rỗng không
            if (equippedSword == null || !equippedSword.HasItem())
            {
                equippedSword = itemToEquip; // 👈 Trang bị vào
                updateEquipped();
                return true;
            }
        }
        // 2. LOGIC TRANG BỊ GIÁP
        else if (type == EquipmentType.Armor)
        {
            // Kiểm tra "socket" giáp có rỗng không
            if (equippedArmor == null || !equippedArmor.HasItem())
            {
                equippedArmor = itemToEquip; // 👈 Trang bị vào
                updateEquipped();
                return true;
            }
        }

        // (Sau này bạn có thể thêm logic cho vào túi đồ)

        // Ổ đã đầy
        return false;
    }

    /// <summary>
    /// Hàm quan trọng: Tính toán lại tất cả chỉ số
    /// </summary>
    public void RecalculateStats()
    {
        // 1. Tính Giáp (Giáp gốc + Giáp từ đồ)
        float armorBonus = (equippedArmor != null && equippedArmor.HasItem()) ? equippedArmor.GetFinalStat() : 0;
        FinalArmour = baseArmour + armorBonus;

        // 2. ⚠️ TÍNH SÁT THƯƠNG THEO CÁCH MỚI ⚠️
        // Lấy sát thương cộng thêm từ kiếm (ví dụ: 15.51)
        FinalBonusDamage = (equippedSword != null && equippedSword.HasItem()) ? equippedSword.GetFinalStat() : 0;

        // Multiplier gốc (sẽ dùng cho buff)
        FinalDamageMultiplier = baseDamageMultiplier;
    }

    /// <summary>
    /// Xuất chỉ số ra console để kiểm tra (ĐÃ CẬP NHẬT)
    /// </summary>
    public void LogStats()
    {
        Debug.Log("========== CẬP NHẬT CHỈ SỐ ==========");

        // ... (Log giáp giữ nguyên) ...
        if (equippedArmor != null && equippedArmor.HasItem())
        {
            // LỖI CŨ: equippedArmor.rarity
            // SỬA LẠI: equippedArmor.rolledRarity.rarityName
            Debug.Log($"GIÁP: [{equippedArmor.GetItemName()} +{equippedArmor.upgradeLevel} ({equippedArmor.rolledRarity.rarityName})] " +
                      $"(Base: {equippedArmor.rolledBaseStat:F2}, Upg: {equippedArmor.upgradeBonusRolls.Sum():F2}, Rarity: {equippedArmor.rolledRarityMultiplier * 100:F0}%) " +
                      $"-> Bonus: <color=cyan>{equippedArmor.GetFinalStat():F2}</color> giáp.");
        }
        else { Debug.Log("GIÁP: [Socket rỗng]"); }
        Debug.Log($"==> TỔNG GIÁP: {baseArmour} (Base) + {(FinalArmour - baseArmour):F2} (Bonus) = <color=white>{FinalArmour:F2}</color>");


        // Thông tin Kiếm (ĐÃ CẬP NHẬT)
        if (equippedSword != null && equippedSword.HasItem())
        {
            // LỖI CŨ: equippedSword.rarity
            // SỬA LẠI: equippedSword.rolledRarity.rarityName
            Debug.Log($"KIẾM: [{equippedSword.GetItemName()} +{equippedSword.upgradeLevel} ({equippedSword.rolledRarity.rarityName})] " +
                      $"(Base: {equippedSword.rolledBaseStat:F2}, Upg: {equippedSword.upgradeBonusRolls.Sum():F2}, Rarity: {equippedSword.rolledRarityMultiplier * 100:F0}%) " + // 👈 Log chi tiết %
                      $"-> Bonus: <color=yellow>+{equippedSword.GetFinalStat():F2}</color> sát thương.");
        }
        else
        {
            Debug.Log("KIẾM: [Socket rỗng]");
        }

        // Log này đã đúng
        Debug.Log($"==> TỔNG SÁT THƯƠNG: (Base Player Dmg + <color=white>{FinalBonusDamage:F2}</color>) * (Buff Multiplier)");
        Debug.Log("======================================");
    }
}