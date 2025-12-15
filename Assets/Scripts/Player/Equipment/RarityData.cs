using UnityEngine;

[CreateAssetMenu(fileName = "NewRarityData", menuName = "Item rarity")]
public class RarityData : ScriptableObject
{
    [Header("Thông tin Rarity")]
    public RarityFrame rarityName; // Enum (Common, Rare,...)
    public Sprite frameSprite; // 👈 Sprite bạn muốn

    [Header("Hệ số nhân (Bonus)")]
    [Tooltip("Hệ số nhân TỐI THIỂU. 1.0 = 100%")]
    public float minStatMultiplier; // 👈 THAY ĐỔI 1: Min

    [Tooltip("Hệ số nhân TỐI ĐA. 1.2 = 120%")]
    public float maxStatMultiplier; // 👈 THAY ĐỔI 2: Max

    // ❌ XÓA: public float statMultiplier;
}