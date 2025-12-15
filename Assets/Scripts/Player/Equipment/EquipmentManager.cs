using UnityEngine;
using System.Collections.Generic;

// Enum định nghĩa các loại trang bị
public enum EquipmentType
{
    Sword,
    Armor,
    // Thêm các loại khác sau này (Helmet, Boots, ...)
}

// Enum định nghĩa các cấp độ hiếm
public enum RarityFrame
{
    Common,
    Uncommon,
    Rare,
    Mythical,
    Legendary,
    Unique
}

/// <summary>
/// Class tĩnh (static) để quản lý các cài đặt chung của trang bị.
/// Không cần kéo thả vào bất kỳ object nào.
/// </summary>
public static class EquipmentManager
{
    /// <summary>
    /// Tính toán tỉ lệ thành công khi nâng cấp.
    /// </summary>
    /// <param name="currentLevel">Cấp độ HIỆN TẠI (từ 0 đến 4)</param>
    public static float GetUpgradeSuccessChance(int currentLevel)
    {
        switch (currentLevel)
        {
            case 0: return 1.0f;  // Lên +1: 100%
            case 1: return 0.8f;  // Lên +2: 80%
            case 2: return 0.6f;  // Lên +3: 60%
            case 3: return 0.4f;  // Lên +4: 40%
            case 4: return 0.2f;  // Lên +5: 20%
            default: return 0f;
        }
    }

    /// <summary>
    /// Tính chi phí nâng cấp.
    /// </summary>
    public static int GetUpgradeCost(int currentLevel)
    {
        // Ví dụ: +1 (100 coin), +2 (250), +3 (500), +4 (1000), +5 (2000)
        return 100 * (int)Mathf.Pow(2.5f, currentLevel);
    }
}