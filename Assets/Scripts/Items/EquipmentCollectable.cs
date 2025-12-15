using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class EquipmentCollectable : MonoBehaviour
{
    // Đây là item "hên xui" thật sự sau khi đã "quay số"
    private EquipmentItem generatedItem;

    private bool isReadyToCollect = false;

    /// <summary>
    /// Hàm này được DropTable gọi NGAY KHI spawn ra "Gói"
    /// </summary>
    public void Initialize(EquipmentData itemTemplate)
    {
        // 1. ⚠️ QUAN TRỌNG: "QUAY SỐ" HÊN XUI ⚠️
        // Gọi hàm static từ file EquipmentInstance của bạn
        generatedItem = EquipmentItem.CreateNewInstance(itemTemplate);

        // 2. Kiểm tra nếu tạo thành công
        if (generatedItem != null && generatedItem.HasItem())
        {
            // (Bạn có thể thêm hiệu ứng particle ở đây để báo Gói xịn/thường)
            isReadyToCollect = true;
        }
        else
        {
            Debug.LogError("Lỗi! Không thể tạo item từ sườn mẫu: " + itemTemplate.name);
            Destroy(gameObject); // Tự hủy nếu không tạo được item
        }
    }

    

    /// <summary>
    /// Xử lý khi Player chạm vào
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chỉ xử lý nếu Gói đã sẵn sàng và là Player
        if (!isReadyToCollect || !collision.CompareTag("Player"))
        {
            return;
        }

        PlayerStats playerStats = collision.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            return;
        }
        // Thêm item vào inventory của Player
        Inventory playerInventory = GameObject.FindAnyObjectByType<Inventory>();
        if (playerInventory != null)
        {
            playerInventory.AddItem(generatedItem.template);
        }

        // 3. 🚀 Gửi item "hên xui" cho PlayerStats
        // (Chúng ta sẽ thêm hàm EquipItem vào PlayerStats ở Bước 3)
        bool equipped = playerStats.EquipItem(generatedItem);

        if (equipped)
        {
            // 4. Nếu trang bị thành công (hoặc cho vào túi)
            Debug.Log($"<color=green>Đã nhặt:</color> {generatedItem.GetItemName()} ({generatedItem.rolledRarity.rarityName})");

            // Tính lại chỉ số và log ra
            playerStats.RecalculateStats();
            playerStats.LogStats();

            // Phá hủy "Gói"
            Destroy(gameObject);
        }
        else
        {
            // Ổ trang bị đã đầy
            Debug.Log($"Ổ {generatedItem.GetItemType()} đã đầy, không thể nhặt!");
        }
    }
}