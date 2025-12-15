using UnityEngine;
using System.Collections.Generic; // 👈 THÊM MỚI: Để dùng List<>
using System.Linq; // 👈 THÊM MỚI: Để dùng .Sum()

[System.Serializable]
public class EquipmentItem
{
    [Header("Sườn Mẫu (Template)")]
    public EquipmentData template; // 👈 Kéo "Sườn Mẫu" vào đây

    [Header("Chỉ số riêng (Instance)")]
    public RarityData rolledRarity; // 👈 Giờ chúng ta lưu cả Sườn Mẫu Rarity
    public int upgradeLevel;

    [Header("Chỉ số ĐÃ RANDOM (Hên Xui)")]
    [Tooltip("Chỉ số gốc được random khi rớt")]
    public float rolledBaseStat; // 👈 Sẽ là 1 số (ví dụ: 10.5)

    [Tooltip("Danh sách các chỉ số bonus đã roll được từ mỗi lần nâng cấp")]
    public List<float> upgradeBonusRolls = new List<float>(); //

    [Tooltip("Hệ số nhân Rarity đã được random khi rớt")]
    public float rolledRarityMultiplier;

    private const int MAX_UPGRADE_LEVEL = 5;

    /// <summary>
    /// Hàm đặc biệt để TẠO MỚI một vật phẩm (khi quái rớt)
    /// </summary>
    public static EquipmentItem CreateNewInstance(EquipmentData data)
    {
        // Kiểm tra xem "Sườn Mẫu Item" có danh sách Rarity không
        if (data == null || data.possibleRarities == null || data.possibleRarities.Length == 0)
        {
            Debug.LogError($"EquipmentData '{data.name}' bị thiếu hoặc không có Rarity nào!");
            return null;
        }

        EquipmentItem newItem = new EquipmentItem();
        newItem.template = data;

        // 1. 👈 TỰ ĐỘNG RANDOM RARITY
        // (Lấy 1 Rarity ngẫu nhiên từ danh sách bạn kéo vào)
        int randomIndex = Random.Range(0, data.possibleRarities.Length);
        newItem.rolledRarity = data.possibleRarities[randomIndex];

        newItem.upgradeLevel = 0;

        // 2. 👈 TỰ ĐỘNG RANDOM CHỈ SỐ GỐC
        newItem.rolledBaseStat = Random.Range(data.minBaseStat, data.maxBaseStat);
        newItem.upgradeBonusRolls = new List<float>(); // 👈 Khởi tạo List rỗng

        // ======== THÊM MỚI: Random % Rarity theo Khoảng ========
        // Quay số % bonus từ min/max của RarityData
        newItem.rolledRarityMultiplier = Random.Range(
            newItem.rolledRarity.minStatMultiplier,
            newItem.rolledRarity.maxStatMultiplier
        );

        return newItem;
    }

    // Constructor rỗng (để Unity serialize)
    public EquipmentItem() { }


    public bool HasItem()
    {
        return template != null;
    }

    public string GetItemName()
    {
        return HasItem() ? template.itemName : "Rỗng";
    }

    /// <summary>
    /// Lấy Sprite khung viền từ Rarity đã random
    /// </summary>
    public Sprite GetFrameSprite()
    {
        return (HasItem() && rolledRarity != null) ? rolledRarity.frameSprite : null;
    }

    /// <summary>
    /// Lấy Loại trang bị (Type) từ sườn mẫu
    /// </summary>
    public EquipmentType GetItemType()
    {
        // Đọc 'type' từ "Sườn Mẫu"
        return template.type;
    }

    /// <summary>
    /// Tính toán chỉ số cuối cùng của món đồ này.
    /// THEO CÔNG THỨC MỚI CỦA BẠN: (Base + Bonus) * Rarity%
    /// </summary>
    public float GetFinalStat()
    {
        if (!HasItem()) return 0f;

        // 1. Lấy tổng chỉ số GỐC + NÂNG CẤP (đã random)
        float totalBase = rolledBaseStat + upgradeBonusRolls.Sum(); // 👈 Lấy tổng từ List

        // 2. ⚠️ THAY ĐỔI: Lấy hệ số nhân Rarity (từ Sườn Mẫu RarityData)
        // ❌ XÓA: float rarityMultiplier = EquipmentManager.GetRarityMultiplier(rarity);
        float rarityMultiplier = rolledRarityMultiplier; // 👈 Lấy % đã random

        // 3. Trả về kết quả cuối cùng
        return totalBase * rarityMultiplier;
    }

    /// <summary>
    /// Thử nâng cấp vật phẩm (ĐÃ CẬP NHẬT LOGIC RANDOM)
    /// </summary>
    public void AttemptUpgrade(PlayerController player)
    {
        if (!HasItem() || player == null || upgradeLevel >= MAX_UPGRADE_LEVEL)
        {
            // ... (log lỗi)
            return;
        }

        int cost = EquipmentManager.GetUpgradeCost(upgradeLevel);
        float successChance = EquipmentManager.GetUpgradeSuccessChance(upgradeLevel);

        if (player.getCoin() < cost)
        {
            Debug.Log($"Không đủ tiền!");
            return;
        }

        player.SpendCoin(cost);
        Debug.Log($"Đã tiêu {cost} coin...");

        if (Random.Range(0f, 1f) <= successChance)
        {
            // THÀNH CÔNG!
            upgradeLevel++;

            // 👈 QUAN TRỌNG: Random chỉ số nâng cấp (Hên Xui)
            float bonusRoll = Random.Range(template.minBonusPerUpgrade, template.maxBonusPerUpgrade);
            upgradeBonusRolls.Add(bonusRoll); // Cộng dồn vào tổng

            Debug.Log($"<color=green>THÀNH CÔNG!</color> [{GetItemName()}] lên +{upgradeLevel}. (Được cộng {bonusRoll:F2} stat!)");
        }
        else
        {
            // THẤT BẠI
            upgradeLevel = Mathf.Max(0, upgradeLevel - 1);
            float removedBonus = 0;
            if (upgradeBonusRolls.Count > 0)
            {
                // Lấy bonus của level cao nhất (cái vừa bị mất)
                removedBonus = upgradeBonusRolls[upgradeBonusRolls.Count - 1];
                // Xóa nó khỏi danh sách
                upgradeBonusRolls.RemoveAt(upgradeBonusRolls.Count - 1);
            }
            Debug.Log($"<color=red>THẤT BẠI!</color> [{GetItemName()}] bị rớt xuống +{upgradeLevel}.");
        }
    }
}