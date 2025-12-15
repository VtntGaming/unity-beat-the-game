using UnityEngine;

public class ShopItemClick : MonoBehaviour
{
    public int slotIndex; // Gán runtime hoặc từ SellItem
    private ShopkeeperController shopkeeper;

    private void Start()
    {
        // Tự tìm shopkeeper gần nhất trong cha
        shopkeeper = GetComponentInParent<ShopkeeperController>();
    }

    private void OnMouseDown()
    {
        if (shopkeeper != null)
        {
            shopkeeper.HandleSlotInteraction(slotIndex);
        }
        else
        {
            Debug.LogWarning("⚠ Không tìm thấy ShopkeeperController!");
        }
    }
}
