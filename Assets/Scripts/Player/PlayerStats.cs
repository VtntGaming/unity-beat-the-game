using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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

    [Header("Inventory")] // 👈 THÊM MỚI: Túi đồ
    public List<EquipmentItem> inventory = new List<EquipmentItem>();
    private const int MAX_INVENTORY_SLOTS = 100; // Ví dụ: Giới hạn 20 ô

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

    // Skill tree
    [Header("Skill listing")]
    public SkillListing skillListing = new SkillListing();

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
    // PlayerStats.cs

    /// <summary>
    /// Thử trang bị một item.
    /// Trả về TRUE nếu trang bị thành công (hoặc cho vào túi).
    /// Trả về FALSE nếu ổ đã đầy VÀ túi đồ đã đầy.
    /// </summary>
    public bool EquipItem(EquipmentItem itemToEquip)
    {
        if (itemToEquip == null || !itemToEquip.HasItem())
        {
            return false;
        }

        EquipmentType type = itemToEquip.GetItemType();

        // === LOGIC TRANG BỊ VÀO SOCKET (Ưu tiên 1) ===
        EquipmentItem currentEquipped = null;

        if (type == EquipmentType.Sword)
        {
            currentEquipped = equippedSword;
            if (equippedSword == null || !equippedSword.HasItem())
            {
                equippedSword = itemToEquip; // Trang bị vào socket rỗng
                RecalculateStats(); // Tính lại chỉ số
                updateEquipped();
                Debug.Log($"Đã trang bị <color=green>{itemToEquip.GetItemName()}</color> vào socket!");
                return true;
            }
        }
        else if (type == EquipmentType.Armor)
        {
            currentEquipped = equippedArmor;
            if (equippedArmor == null || !equippedArmor.HasItem())
            {
                equippedArmor = itemToEquip; // Trang bị vào socket rỗng
                RecalculateStats();
                updateEquipped();
                Debug.Log($"Đã trang bị <color=green>{itemToEquip.GetItemName()}</color> vào socket!");
                return true;
            }
        }

        // === LOGIC CHUYỂN VÀO TÚI ĐỒ (Ưu tiên 2) ===
        // Nếu socket đã đầy (currentEquipped != null), chuyển vào túi đồ.
        if (inventory.Count < MAX_INVENTORY_SLOTS)
        {
            inventory.Add(itemToEquip);
            Debug.Log($"Đã thêm <color=yellow>{itemToEquip.GetItemName()}</color> vào Túi Đồ ({inventory.Count}/{MAX_INVENTORY_SLOTS})");
            return true;
        }

        // === LOGIC THẤT BẠI (Ưu tiên 3) ===
        // Ổ đã đầy và túi đồ cũng đầy
        Debug.LogWarning($"❌ Túi đồ đã đầy ({MAX_INVENTORY_SLOTS} món). <color=red>{itemToEquip.GetItemName()}</color> bị bỏ lại!");
        return false;
    }
    // PlayerStats.cs

    /// <summary>
    /// Trao đổi vật phẩm đang trang bị với vật phẩm trong Inventory.
    /// </summary>
    public void SwapItem(EquipmentItem itemFromInventory)
    {
        if (itemFromInventory == null || !itemFromInventory.HasItem() || !inventory.Contains(itemFromInventory))
        {
            Debug.LogError("Vật phẩm không hợp lệ hoặc không có trong túi đồ.");
            return;
        }

        EquipmentType type = itemFromInventory.GetItemType();
        EquipmentItem currentlyEquipped = null;

        if (type == EquipmentType.Sword)
        {
            currentlyEquipped = equippedSword;
            equippedSword = itemFromInventory; // Trang bị vật phẩm mới
        }
        else if (type == EquipmentType.Armor)
        {
            currentlyEquipped = equippedArmor;
            equippedArmor = itemFromInventory; // Trang bị vật phẩm mới
        }
        else
        {
            Debug.LogError("Loại vật phẩm không thể trang bị (Chưa có socket).");
            return;
        }

        // 1. Xóa vật phẩm mới ra khỏi túi đồ
        inventory.Remove(itemFromInventory);

        // 2. Đặt vật phẩm CŨ đang trang bị vào túi đồ (nếu có)
        if (currentlyEquipped != null && currentlyEquipped.HasItem())
        {
            inventory.Add(currentlyEquipped);
            Debug.Log($"Đã thay <color=yellow>{itemFromInventory.GetItemName()}</color> bằng <color=yellow>{currentlyEquipped.GetItemName()}</color> (chuyển vào túi đồ).");
        }
        else
        {
            Debug.Log($"Đã trang bị <color=green>{itemFromInventory.GetItemName()}</color> (không có vật phẩm cũ để trao đổi).");
        }

        RecalculateStats(); // Tính lại chỉ số sau khi thay đổi
        updateEquipped(); // Cập nhật UI
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