using UnityEngine;

[CreateAssetMenu(fileName = "New item", menuName = "Item/Basic item")]
public class ItemData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string itemName;
    public EquipmentType type;
    public Sprite itemSprite;

    [Header("Danh sách Rarity có thể rớt")]
    [Tooltip("Kéo các file RarityData.asset vào đây")]
    public RarityData[] possibleRarities;
}
