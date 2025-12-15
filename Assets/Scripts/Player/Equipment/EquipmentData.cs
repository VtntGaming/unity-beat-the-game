using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipmentData", menuName = "Item/Equipment item")]
public class EquipmentData : ItemData
{
    // ❌ XÓA CÁC BIẾN CŨ: baseStat, bonusPerUpgrade

    [Header("Chỉ số Random GỐC (Khi rớt)")]
    public float minBaseStat; // Ví dụ: Kiếm Gỗ (8)
    public float maxBaseStat; // Ví dụ: Kiếm Gỗ (12)

    [Header("Chỉ số Random NÂNG CẤP (Mỗi cấp +1)")]
    public float minBonusPerUpgrade; // Ví dụ: +1
    public float maxBonusPerUpgrade; // Ví dụ: +3
}